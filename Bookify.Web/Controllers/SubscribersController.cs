
using Bookify.Web.Core.Models;

namespace Bookify.Web.Controllers
{
    public class SubscribersController : Controller
    {
        private readonly ISubscribersRepository _subscribersRepo;
        private readonly IGovernorateRepo _governorateRepo;
        private readonly IAreaRepo _areaRepo;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        public SubscribersController(
            ISubscribersRepository subscribersRepository,
            IGovernorateRepo governorateRepo,
            IAreaRepo areaRepo,
            IMapper mapper,
            IImageService imageService)
        {
            _subscribersRepo = subscribersRepository;
            _governorateRepo = governorateRepo;
            _areaRepo = areaRepo;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var subscribers = await _subscribersRepo.GetAllAsync();
            var subscriberViewModels = _mapper.Map<IEnumerable<SubscriberViewModel>>(subscribers);
            return View(subscriberViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await PopulateViewModel();
            return View("Form", model);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {

            var existingSubscriber = await _subscribersRepo.GetByIdAsync(Id);

            if (existingSubscriber == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<SubscriberFormViewModel>(existingSubscriber);
            var model = await PopulateViewModel(viewModel);
            return View("Form", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            var existingSubscriber = await _subscribersRepo.GetByIdAsync(model.Id);


            if (!ModelState.IsValid)
            {
                var viewModel = await PopulateViewModel(model);
                return View("Form", viewModel);
            }

            _mapper.Map(model, existingSubscriber);
         
            // Handle image update
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
                    await PopulateViewModel(model);
                    ModelState.AddModelError(string.Empty, "Failed to save the image. Please try again.");
                    return View("Form", model);
                }
            }

            existingSubscriber.LastUpdatedOn = DateTime.Now;
            existingSubscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;


            await _subscribersRepo.UpdateAsync(existingSubscriber);

            TempData["SuccessMessage"] = "Subscriber Updated successfully!";

            return RedirectToAction(nameof(Index));


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await PopulateViewModel(model);
                return View("Form", viewModel);
            }


            var newSubscriber = _mapper.Map<Subscriber>(model);
            newSubscriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Handle image upload
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

                    var viewModel = await PopulateViewModel(model);
                    return View("Form", viewModel);

                }
            }



            await _subscribersRepo.AddAsync(newSubscriber);

            TempData["SuccessMessage"] = "Subscriber added successfully!";

            return RedirectToAction(nameof(Index));
        }




        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int Id)
        {

            var subscriber = await _subscribersRepo.GetByIdAsync(Id);
            if (subscriber == null)
            {
                return NotFound();

            }

            subscriber.IsDeleted = !subscriber.IsDeleted;
            subscriber.LastUpdatedOn = DateTime.Now;
            subscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            await _subscribersRepo.UpdateAsync(subscriber);

            TempData["SuccessMessage"] = "Subscriber status updated successfully!";

            return RedirectToAction(nameof(Index));

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existingSubscriber = await _subscribersRepo.GetByIdAsync(id);
            if (existingSubscriber == null) return Json(new { success = false, message = "Subscriber not found." });

            await _subscribersRepo.DeleteAsync(existingSubscriber.Id);

            return Json(new { success = true });
        }


        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> AllowEmail(SubscriberFormViewModel model)
        {
            var subscriber = await _subscribersRepo.FindSubscriberAsync(s => s.Email == model.Email);
            var isAllowed = subscriber == null || subscriber.Id.Equals(model.Id);
            return Json(isAllowed);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> AllowMobileNumber(SubscriberFormViewModel model)
        {
            var subscriber = await _subscribersRepo.FindSubscriberAsync(s => s.MobileNumber == model.MobileNumber);
            var isAllowed = subscriber == null || subscriber.Id.Equals(model.Id);
            return Json(isAllowed);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriber = await _subscribersRepo.FindSubscriberAsync(s => s.NationalId == model.NationalId);
            var isAllowed = subscriber == null || subscriber.Id.Equals(model.Id);
            return Json(isAllowed);
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
                viewModel.Areas = new List<SelectListItem>(); // No areas if no governorate is selected
            }

            return viewModel;
        }

        [HttpGet]
        public async Task<IActionResult> GetCitiesByGovernorate(int governorateId)
        {
            var areas = await _areaRepo.GetAreasByGovernorateIdAsync(governorateId);
            var areaList = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            return Json(areaList);
        }
    }
}
