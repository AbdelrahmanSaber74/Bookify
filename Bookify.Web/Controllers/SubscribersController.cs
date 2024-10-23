namespace Bookify.Web.Controllers
{
    public class SubscribersController : Controller
    {
        private readonly ISubscribersRepository _subscribersRepo;
        private readonly IGovernorateRepo _governorateRepo;
        private readonly IAreaRepo _areaRepo;
        private readonly IMapper _mapper;

        public SubscribersController(
            ISubscribersRepository subscribersRepository,
            IGovernorateRepo governorateRepo,
            IAreaRepo areaRepo,
            IMapper mapper)
        {
            _subscribersRepo = subscribersRepository;
            _governorateRepo = governorateRepo;
            _areaRepo = areaRepo;
            _mapper = mapper;
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


        [HttpPost]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {

            return Json(model);

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
            viewModel.Areas = new List<SelectListItem>();

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
