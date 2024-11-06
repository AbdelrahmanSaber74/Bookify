using Bookify.Web.Repositories.Subscription;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using WhatsAppCloudApi.Services;

namespace Bookify.Web.Controllers
{
    public class SubscribersController : Controller
    {
        private readonly ISubscribersRepo _subscribersRepo;
        private readonly IGovernorateRepo _governorateRepo;
        private readonly IAreaRepo _areaRepo;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IDataProtector _dataProtector;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly ISubscriptionRepo _subscriptionRepo;
        private readonly NotificationService _notificationService;


        public SubscribersController(
            ISubscribersRepo subscribersRepo,
            IGovernorateRepo governorateRepo,
            IAreaRepo areaRepo,
            IMapper mapper,
            ApplicationDbContext applicationDbContext,
            IImageService imageService,
            IDataProtectionProvider dataProtectionProvider,
            IWhatsAppClient whatsAppClient,
            IWebHostEnvironment webHostEnvironment,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender,
            ISubscriptionRepo subscriptionRepo,
            NotificationService notificationService)
        {
            _subscribersRepo = subscribersRepo;
            _governorateRepo = governorateRepo;
            _areaRepo = areaRepo;
            _mapper = mapper;
            _imageService = imageService;
            _dataProtector = dataProtectionProvider.CreateProtector("MySecureKey");
            _whatsAppClient = whatsAppClient;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _subscriptionRepo = subscriptionRepo;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index() => View();


        [HttpPost]
        public async Task<IActionResult> Search(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return View("Index", null);
            }

            var subscriber = await _subscribersRepo.Search(value);
            if (subscriber != null)
            {
                var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());
                return View("Index", viewModel);
            }

            return View("Index", null);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = await _subscribersRepo.GetByIdAsync(subscriberId);

            if (subscriber == null)
            {
                return RedirectToAction(actionName: "Index");
            }

            if (subscriber.IsDeleted)
            {
                TempData["WarningMessage"] = "Subscriber Is Deleted!";
                return RedirectToAction(actionName: "Index");

            }

            var subscriberViewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            subscriberViewModel.Key = id;
            return View("Details", subscriberViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View("Form", await PopulateViewModel());

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var existingSubscriber = await _subscribersRepo.GetByIdAsync(subscriberId);

            if (existingSubscriber == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<SubscriberFormViewModel>(existingSubscriber);
            viewModel.Key = id;
            return View("Form", await PopulateViewModel(viewModel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));
            var existingSubscriber = await _subscribersRepo.GetByIdAsync(subscriberId);

            if (!ModelState.IsValid)
            {
                return View("Form", await PopulateViewModel(model));
            }

            _mapper.Map(model, existingSubscriber);
            await HandleImageUpdate(model, existingSubscriber);
            existingSubscriber.LastUpdatedOn = DateTime.Now;
            existingSubscriber.LastUpdatedById = GetCurrentUserId();

            await _subscribersRepo.UpdateAsync(existingSubscriber);
            TempData["SuccessMessage"] = "Subscriber Updated successfully!";
            return RedirectToAction(nameof(Details), new { id = model.Key });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(" Form", await PopulateViewModel(model));
            }

            var newSubscriber = _mapper.Map<Subscriber>(model);
            newSubscriber.CreatedById = GetCurrentUserId();
            await HandleImageUpload(model, newSubscriber);



            // Create and add subscription
            var subscription = CreateSubscription(newSubscriber.Id, GetCurrentUserId());
            newSubscriber.subscriptions.Add(subscription);

            await _subscribersRepo.AddAsync(newSubscriber);


            model.Image = null;
            BackgroundJob.Enqueue(() => _notificationService.SendWhatsAppNotification(model));
            BackgroundJob.Enqueue(() => _notificationService.SendWelcomeEmail(model));



            TempData["SuccessMessage"] = "Subscriber added successfully!";
            return RedirectToAction(nameof(Details), new { id = _dataProtector.Protect(newSubscriber.Id.ToString()) });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var subscriber = await _subscribersRepo.GetByIdAsync(id);
            if (subscriber == null)
            {
                return NotFound();
            }

            subscriber.IsDeleted = !subscriber.IsDeleted;
            subscriber.LastUpdatedOn = DateTime.Now;
            subscriber.LastUpdatedById = GetCurrentUserId();

            await _subscribersRepo.UpdateAsync(subscriber);
            TempData["SuccessMessage"] = "Subscriber status updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existingSubscriber = await _subscribersRepo.GetByIdAsync(id);
            if (existingSubscriber == null)
            {
                return Json(new { success = false, message = "Subscriber not found." });
            }

            await _subscribersRepo.DeleteAsync(existingSubscriber.Id);
            return Json(new { success = true });
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> AllowEmail(SubscriberFormViewModel model)
        {
            var subscriberId = string.IsNullOrEmpty(model.Key) ? 0 : int.Parse(_dataProtector.Unprotect(model.Key));
            var subscriber = await _subscribersRepo.FindSubscriberAsync(s => s.Email == model.Email);
            return Json(subscriber == null || subscriber.Id.Equals(subscriberId));
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> AllowMobileNumber(SubscriberFormViewModel model)
        {
            var subscriberId = string.IsNullOrEmpty(model.Key) ? 0 : int.Parse(_dataProtector.Unprotect(model.Key));
            var subscriber = await _subscribersRepo.FindSubscriberAsync(s => s.MobileNumber == model.MobileNumber);
            return Json(subscriber == null || subscriber.Id.Equals(subscriberId));
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriberId = string.IsNullOrEmpty(model.Key) ? 0 : int.Parse(_dataProtector.Unprotect(model.Key));
            var subscriber = await _subscribersRepo.FindSubscriberAsync(s => s.NationalId == model.NationalId);
            return Json(subscriber == null || subscriber.Id.Equals(subscriberId));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewSubscription(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));


            //  Last end date Subscription
            var subscription = await _subscriptionRepo.GetLastSubscriptionBySubscriberId(subscriberId);

            var subscriber = await _subscribersRepo.GetByIdAsync(subscriberId);

            if (subscription == null)
            {
                return NotFound();
            }


            var startDate = subscription.EndDate > DateTime.Now ? subscription.EndDate.AddDays(1) : DateTime.Today;


            // Create and add subscription
            var newSubscription = CreateSubscription(subscriberId, GetCurrentUserId()!, startDate);
            await _subscriptionRepo.AddSubscription(newSubscription);


            //Send Email And Whatapp
            var subscriberModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
            BackgroundJob.Enqueue(() => _notificationService.SendWhatsAppNotification(subscriberModel));
            BackgroundJob.Enqueue(() => _notificationService.SendWelcomeEmail(subscriberModel));


            var subscriptionModel = _mapper.Map<SubscriptionViewModel>(newSubscription);

            return PartialView("_SubscriptionRow", subscriptionModel);

        }

        private async Task<SubscriberFormViewModel> PopulateViewModel(SubscriberFormViewModel? model = null)
        {
            var viewModel = model ?? new SubscriberFormViewModel();
            var governorates = await _governorateRepo.GetAllGovernoratesAsync() ?? new List<Governorate>();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            if (viewModel.GovernorateId > 0)
            {
                var areas = await _areaRepo.GetAreasByGovernorateIdAsync(viewModel.GovernorateId);
                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }
            else
            {
                viewModel.Areas = new List<SelectListItem>();
            }

            return viewModel;
        }

        private async Task HandleImageUpdate(SubscriberFormViewModel model, Subscriber existingSubscriber)
        {
            if (model.Image != null)
            {
                if (!string.IsNullOrEmpty(existingSubscriber.ImageThumbnailUrl) || !string.IsNullOrEmpty(existingSubscriber.ImageUrl))
                {
                    _imageService.DeleteOldImages(existingSubscriber.ImageUrl, existingSubscriber.ImageThumbnailUrl);
                }

                var saveImageResult = await _imageService.SaveImageAsync(model.Image, "images/Subscribers", true);
                if (saveImageResult is OkObjectResult okResult)
                {
                    var resultData = (dynamic)okResult.Value;
                    existingSubscriber.ImageUrl = resultData.relativePath;
                    existingSubscriber.ImageThumbnailUrl = resultData.thumbnailRelativePath;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to save the image. Please try again.");
                }
            }
        }

        private async Task HandleImageUpload(SubscriberFormViewModel model, Subscriber newSubscriber)
        {
            if (model.Image != null)
            {
                var saveImageResult = await _imageService.SaveImageAsync(model.Image, "images/Subscribers", true);
                if (saveImageResult is OkObjectResult okResult)
                {
                    var resultData = (dynamic)okResult.Value;
                    newSubscriber.ImageUrl = resultData.relativePath;
                    newSubscriber.ImageThumbnailUrl = resultData.thumbnailRelativePath;
                }
                else if (saveImageResult is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString() ?? Errors.ImageSaveFailed;
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
            }
        }


        private Subscription CreateSubscription(int subscriberId, string createdById, DateTime? startDate = null)
        {
            startDate ??= DateTime.Now;

            return new Subscription
            {
                SubscriberId = subscriberId,
                CreatedById = createdById,
                CreatedOn = DateTime.Now,
                StartDate = startDate.Value,
                EndDate = startDate.Value.AddYears(1)

            };
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }

        [HttpGet]
        public async Task<IActionResult> GetAreasByGovernorate(int governorateId)
        {
            var areas = await _areaRepo.GetAreasByGovernorateIdAsync(governorateId);
            var areaList = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            return Json(areaList);
        }
    }
}