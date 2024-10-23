using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Web.Controllers
{
    public class SubscribersController : Controller
    {
        private readonly ISubscribersRepository _subscribersRepository;
        private readonly IGovernorateRepo _governorateRepo;
        private readonly IAreaRepo _areaRepo;
        private readonly IMapper _mapper;

        public SubscribersController(
            ISubscribersRepository subscribersRepository,
            IGovernorateRepo governorateRepo,
            IAreaRepo areaRepo,
            IMapper mapper)
        {
            _subscribersRepository = subscribersRepository;
            _governorateRepo = governorateRepo;
            _areaRepo = areaRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var subscribers = await _subscribersRepository.GetAllAsync();
            var subscriberViewModels = _mapper.Map<IEnumerable<SubscriberViewModel>>(subscribers);
            return View(subscriberViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await PopulateViewModel();
            return View("Form", model);
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
