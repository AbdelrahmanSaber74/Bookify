namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly IBookCopyRepo _bookCopyRepo;
        private readonly IMapper _mapper;
        private readonly ISubscribersRepo _subscribersRepo;
        private readonly IDataProtector _dataProtector;

        public RentalsController(IBookCopyRepo bookCopyRepo, IMapper mapper, ISubscribersRepo subscribersRepo, IDataProtectionProvider dataProtectionProvider)
        {
            _bookCopyRepo = bookCopyRepo;
            _mapper = mapper;
            _subscribersRepo = subscribersRepo;
            _dataProtector = dataProtectionProvider.CreateProtector("MySecureKey");
        }

        [HttpGet]
        public async Task<IActionResult> Create(string sKey)
        {
            // Decrypt subscriber ID
            var subscriberId = DecryptSubscriberId(sKey);
         
            // Fetch subscriber details
            var subscriber = await _subscribersRepo.GetByIdAsync(subscriberId.Value);

            // Check if subscriber is blacklisted or inactive or notfount
            var validationResult = ValidateSubscriber(subscriber);
            if (validationResult != null)
            {
                return View("NotAllowedRental", validationResult);
            }


            var model = new RentalFormViewModel
            {
                SubscriberKey = sKey
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetCopyDetails(SearchFormWithKeyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Decrypt subscriber ID
            var subscriberId = DecryptSubscriberId(model.SubscriberKey);

            // Fetch subscriber details
            var subscriber = await _subscribersRepo.GetByIdAsync(subscriberId.Value);

            // Check if subscriber is blacklisted or inactive or notfount
            var validationResult = ValidateSubscriber(subscriber);
            if (validationResult != null)
            {
                return View("NotAllowedRental", validationResult);
            }

            // Fetch book copy details
            var bookCopy = await _bookCopyRepo.GetCopyDetails(model.SearchForm.Value);
            if (bookCopy == null || !IsBookAvailableForRental(bookCopy))
            {
                TempData["WarningMessage"] = "The specified book copy or its main book is not available for rental.";
                return RedirectToAction("Create", new { subscriberKey = model.SubscriberKey });
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
        public async Task<IActionResult> Create(RentalFormViewModel model)
        {
            return Json(model);
        }

        private int? DecryptSubscriberId(string sKey)
        {
            try
            {
                var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));
                return subscriberId;
            }
            catch
            {
                return null;
            }
        }

        private string? ValidateSubscriber(Subscriber subscriber)
        {
            if (subscriber == null)
            {
                return Errors.SubscriberNotFound;

            }

            if (subscriber.IsBlackListed)
            {
                return Errors.BlackListedSubscriber;
            }

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
    }
}