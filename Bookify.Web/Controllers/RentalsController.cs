using Bookify.Web.Core.Models;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly IBookCopyRepo _bookCopyRepo;
        private readonly IRentalRepo _rentalRepo;
        private readonly IRentalCopyRepo _rentalCopyRepo;
        private readonly IMapper _mapper;
        private readonly ISubscribersRepo _subscribersRepo;
        private readonly IDataProtector _dataProtector;

        public RentalsController(
            IBookCopyRepo bookCopyRepo,
            IMapper mapper,
            ISubscribersRepo subscribersRepo,
            IDataProtectionProvider dataProtectionProvider,
            IRentalRepo rentalRepo,
            IRentalCopyRepo rentalCopyRepo)
        {
            _bookCopyRepo = bookCopyRepo;
            _mapper = mapper;
            _subscribersRepo = subscribersRepo;
            _dataProtector = dataProtectionProvider.CreateProtector("MySecureKey");
            _rentalRepo = rentalRepo;
            _rentalCopyRepo = rentalCopyRepo;
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsDeleted(int id)
        {
            var rental = await _rentalRepo.GetByIdAsync(id);

            if (rental == null || rental.CreatedOn.Date != DateTime.Today)
            {
                return NotFound();
            }

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _rentalRepo.UpdateAsync(rental);

            var copiesCount = await _rentalCopyRepo.CountRentalCopiesByIdAsync(rental.Id);

            return Ok(new { count = copiesCount });
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {

            var rental = await _rentalRepo.GetRentalWithCopiesByIdAsync(id);

            if (rental is null) return NotFound();

            var rentalViewModel = _mapper.Map<RentalViewModel>(rental);


            return View(rentalViewModel);

        }


        [HttpGet]
        public async Task<IActionResult> Return(int id)
        {
            var rental = await GetRentalAsync(id);
            if (rental == null) return NotFound();

            var subscriber = await _subscribersRepo.GetByIdAsync(rental.SubscriberId);
            var validationResult = ValidateSubscriber(subscriber);
            if (validationResult != null) return View("NotAllowedRental", validationResult);

            var viewModel = BuildRentalReturnFormViewModel(id, rental, subscriber);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Return(RentalReturnFormViewModel model)
        {
            var rental = await GetRentalAsync(model.Id);
            if (rental == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(r => !r.ReturnDate.HasValue).ToList());
                return View(model);
            }

            var subscriber = await _subscribersRepo.GetByIdAsync(rental.SubscriberId);
            var validationResult = ValidateSubscriber(subscriber);
            if (validationResult != null) return View("NotAllowedRental", validationResult);

            if (await IsRentalReturnNotAllowedAsync(subscriber, rental, model.SelectedCopies))
            {
                model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).ToList());
                return View(model);
            }



            var isUpdate = false;

            foreach (var rentalCopy in model.SelectedCopies)
            {
                var currentCopy = await _rentalCopyRepo.GetByBookCopyIdAsync(rental.Id, rentalCopy.Id);



                if (rentalCopy.IsReturned.HasValue && rentalCopy.IsReturned.Value)
                {
                    if (currentCopy.ReturnDate.HasValue) continue;

                    currentCopy.ReturnDate = DateTime.Now;
                    isUpdate = true;
                    await _rentalCopyRepo.UpdateAsync(currentCopy);

                }

                if (rentalCopy.IsReturned.HasValue && !rentalCopy.IsReturned.Value)
                {
                    if (currentCopy.ExtendedOn.HasValue) continue;

                    currentCopy.ExtendedOn = DateTime.Now;
                    currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalsConfigurations.MaxRentalDuration);
                    isUpdate = true;

                    await _rentalCopyRepo.UpdateAsync(currentCopy);

                }


            }


            if (isUpdate)
            {
                rental.LastUpdatedOn = DateTime.Now;
                rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                rental.PenaltyPaid = model.PenaltyPaid;
                await _rentalRepo.UpdateAsync(rental);
            }



            return RedirectToAction(nameof(Details), new { id = rental.Id });

        }







        #region Private Methods

        private async Task<Rental> GetRentalAsync(int id)
        {
            return await _rentalRepo.GetRentalWithCopiesByIdAsync(id);
        }

        private RentalReturnFormViewModel BuildRentalReturnFormViewModel(int id, Rental rental, Subscriber subscriber)
        {
            return new RentalReturnFormViewModel
            {
                Id = id,
                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(r => !r.ReturnDate.HasValue).ToList()),
                SelectedCopies = rental.RentalCopies.Select(c => new ReturnCopyViewModel
                {
                    Id = c.BookCopyId,
                    IsReturned = c.ExtendedOn.HasValue ? false : null
                }).ToList(),
                AllowExtend = CanExtendRental(subscriber, rental)
            };
        }

        private bool CanExtendRental(Subscriber subscriber, Rental rental)
        {
            return !subscriber.IsBlackListed
                   && subscriber.subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration)
                   && rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration) >= DateTime.Today;
        }

        private async Task<bool> IsRentalReturnNotAllowedAsync(Subscriber subscriber, Rental rental, IList<ReturnCopyViewModel> selectedCopies)
        {
            if (selectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            {
                if (subscriber.IsBlackListed)
                {
                    ModelState.AddModelError("", Errors.RentalNotAllowedForBlacklisted);
                    return true;
                }

                if (IsSubscriptionInactive(subscriber, rental))
                {
                    ModelState.AddModelError(string.Empty, Errors.RentalNotAllowedForInactive);
                    return true;
                }

                if (IsRentalOverdue(rental))
                {
                    ModelState.AddModelError(string.Empty, Errors.ExtendNotAllowed);
                    return true;
                }
            }

            return false;
        }

        private bool IsSubscriptionInactive(Subscriber subscriber, Rental rental)
        {
            return subscriber.subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration);
        }

        private bool IsRentalOverdue(Rental rental)
        {
            return rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration) < DateTime.Today;
        }


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
