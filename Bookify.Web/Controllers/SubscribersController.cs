using Bookify.Web.Repositories.Subscription;
using Microsoft.AspNetCore.DataProtection;
using WhatsAppCloudApi;
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
            ISubscriptionRepo subscriptionRepo)
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
            await SendWhatsAppNotification(model);
            await SendWelcomeEmail(model);


            // Create and add subscription
            var subscription = CreateSubscription(newSubscriber.Id, GetCurrentUserId());
            newSubscriber.subscriptions.Add(subscription);

            await _subscribersRepo.AddAsync(newSubscriber);
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
            await SendWhatsAppNotification(subscriberModel);
            await SendWelcomeEmail(subscriberModel);


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

        private async Task SendWhatsAppNotification(SubscriberFormViewModel model)
        {
            if (model.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                {
                    new WhatsAppComponent
                    {
                        Type = "body",
                        Parameters = new List<object>()
                        {
                            new WhatsAppTextParameter { Text = $"{model.FirstName}" }
                        }
                    }
                };

                var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01022917856" : model.MobileNumber;

                var result = await _whatsAppClient.SendMessage(
                    $"2{mobileNumber}",
                    WhatsAppLanguageCode.English_US,
                    WhatsAppTemplates.statement_available_2,
                    components
                );


            }
        }

        private async Task SendWelcomeEmail(SubscriberFormViewModel model)
        {
            var placeholder = new Dictionary<string, string>()
            {
                { "imageUrl" , "https://res.cloudinary.com/dkbsaseyc/image/upload/fl_preserve_transparency/v1729557070/icon-positive-vote-2_jcxdww_rghb1a.jpg?_s=public-apps"} ,
                { "header", $"Welcome {model.FirstName}," },
                { "body", "thanks for joining Bookify 🤩" }
            };

            var body = await _emailBodyBuilder.GetEmailBodyAsync(
                EmailTemplates.Notification,
                placeholder
            );

            await _emailSender.SendEmailAsync(model.Email, "Confirm your email", body);
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