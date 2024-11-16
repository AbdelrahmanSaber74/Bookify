namespace Bookify.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IMapper _mapper;
		private readonly IBookRepo _bookRepo;
		private readonly IDataProtector _dataProtector;

		public HomeController(ILogger<HomeController> logger, IMapper mapper, IBookRepo bookRepo , IDataProtectionProvider dataProtectionProvider)
		{
			_logger = logger;
			_mapper = mapper;
			_bookRepo = bookRepo;
			_dataProtector = dataProtectionProvider.CreateProtector("MySecureKey");
		}

		public async Task<IActionResult> Index()
		{

			if (User.Identity!.IsAuthenticated)
			{
				return RedirectToAction("Index" , "Dashboard");
			}


			var books = await _bookRepo.LastAddedBooks();

			var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(books);

            foreach (var book in viewModel)
            {
                book.Key = _dataProtector.Protect(book.Id.ToString());
			}

			return View(viewModel);
		}




	}
}
