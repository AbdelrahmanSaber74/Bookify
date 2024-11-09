using Bookify.Web.Repositories.RentalCopies;
using Bookify.Web.Repositories.Rentals;


namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly IBookCopyRepo _bookCopyRepo;
        private readonly IRentalRepo _rentalRepo;
        private readonly IMapper _mapper;
        private readonly ISubscribersRepo _subscribersRepo;
        private readonly IDataProtector _dataProtector;

        public RentalsController(
            IBookCopyRepo bookCopyRepo,
            IMapper mapper,
            ISubscribersRepo subscribersRepo,
            IDataProtectionProvider dataProtectionProvider,
            IRentalRepo rentalRepo)
        {
            _bookCopyRepo = bookCopyRepo;
            _mapper = mapper;
            _subscribersRepo = subscribersRepo;
            _dataProtector = dataProtectionProvider.CreateProtector("MySecureKey");
            _rentalRepo = rentalRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Create(string sKey)
        {
            var subscriberId = DecryptSubscriberId(sKey);
            if (subscriberId == null)
            {
                return View("NotAllowedRental", Errors.InvalidSubscriberId);
            }

            var subscriber = await _subscribersRepo.GetByIdAsync(subscriberId.Value);
            var validationResult = ValidateSubscriber(subscriber);
            if (validationResult != null)
            {
                return View("NotAllowedRental", validationResult);
            }

            var model = new RentalFormViewModel { SubscriberKey = sKey };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Create", new { sKey = model.SubscriberKey });
            }

            var subscriberId = DecryptSubscriberId(model.SubscriberKey);
            if (subscriberId == null)
            {
                return RedirectToAction("Create", new { sKey = model.SubscriberKey });
            }

            var subscriber = await _subscribersRepo.GetByIdAsync(subscriberId.Value);
            var validationResult = ValidateSubscriber(subscriber);
            if (validationResult != null)
            {
                return View("NotAllowedRental", validationResult);
            }

            var bookCopy = await _bookCopyRepo.GetBookCopyByIdAsync(model.BookCopyId);
            if (bookCopy == null || !IsBookAvailableForRental(bookCopy))
            {
                TempData["WarningMessage"] = Errors.BookCopyUnavailable;
                return RedirectToAction("Create", new { sKey = model.SubscriberKey });
            }

            if (await IsBookCopyAlreadyRented(subscriberId.Value, model.BookCopyId))
            {
                TempData["ErrorMessage"] = Errors.BookAlreadyRented;
                return RedirectToAction("Create", new { sKey = model.SubscriberKey });
            }

            var rental = CreateRental(subscriberId.Value, model.BookCopyId);
            await _rentalRepo.AddAsync(rental);

            TempData["SuccessMessage"] = Errors.RentalSuccessMessage;
            return RedirectToAction("Create", new { sKey = model.SubscriberKey });
        }

        [HttpPost]
        public async Task<IActionResult> GetCopyDetails(SearchFormWithKeyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subscriberId = DecryptSubscriberId(model.SubscriberKey!);
            if (subscriberId == null)
            {
                return BadRequest(Errors.InvalidSubscriberId);
            }

            var subscriber = await _subscribersRepo.GetByIdAsync(subscriberId.Value);
            var validationResult = ValidateSubscriber(subscriber);
            if (validationResult != null)
            {
                return View("NotAllowedRental", validationResult);
            }

            var bookCopy = await _bookCopyRepo.GetCopyDetails(model.SearchForm.Value);
            if (bookCopy == null || !IsBookAvailableForRental(bookCopy))
            {
                TempData["WarningMessage"] = Errors.BookCopyUnavailable;
                return RedirectToAction("Create", new { sKey = model.SubscriberKey });
            }

            if (await IsBookCopyAlreadyRented(subscriberId.Value, bookCopy.Id))
            {
                TempData["WarningMessage"] = Errors.CopyIsInRental;
                return RedirectToAction("Create", new { sKey = model.SubscriberKey });
            }

            var bookCopyDetails = _mapper.Map<BookCopyViewModel>(bookCopy);
            var rentalFormViewModel = new RentalFormViewModel
            {
                SubscriberKey = model.SubscriberKey,
                BookCopyDetails = bookCopyDetails,
                BookCopyId = bookCopy.Id
            };

            return View("Create", rentalFormViewModel);
        }

        #region Private Methods

        private int? DecryptSubscriberId(string sKey)
        {
            try
            {
                return int.Parse(_dataProtector.Unprotect(sKey));
            }
            catch
            {
                return null;
            }
        }

        private string? ValidateSubscriber(Subscriber subscriber)
        {
            if (subscriber == null) return Errors.SubscriberNotFound;
            if (subscriber.IsBlackListed) return Errors.BlackListedSubscriber;

            var currentRentalCopies = subscriber.Rentals
                .SelectMany(r => r.RentalCopies)
                .Count(c => !c.ReturnDate.HasValue);

            if (currentRentalCopies > 3) return Errors.MaxCopiesReached;

            if (subscriber.subscriptions.LastOrDefault()?.EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
            {
                return Errors.InactiveSubscriber;
            }

            return null;
        }

        private bool IsBookAvailableForRental(BookCopy bookCopy)
        {
            return bookCopy.IsAvailableForRental && bookCopy.Book.IsAvailableForRental;
        }

        private async Task<bool> IsBookCopyAlreadyRented(int subscriberId, int bookCopyId)
        {
            var subscriberRentals = await _rentalRepo.GetAllBySubscriberIdAsync(subscriberId);
            return subscriberRentals
                .Any(rental => rental.RentalCopies
                .Any(rentalCopy => rentalCopy.BookCopyId == bookCopyId));
        }

        private Rental CreateRental(int subscriberId, int bookCopyId)
        {
            return new Rental
            {
                SubscriberId = subscriberId,
                StartDate = DateTime.Now,
                RentalCopies = new List<RentalCopy>
                {
                    new RentalCopy
                    {
                        BookCopyId = bookCopyId,
                        RentalDate = DateTime.Now,
                        EndDate = DateTime.Now.AddDays(7),
                        CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        CreatedOn = DateTime.Now
                    }
                }
            };
        }

        #endregion
    }
}
