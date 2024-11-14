namespace Bookify.Web.Controllers
{
	public class DashboardController : Controller
	{

		private readonly IBookCopyRepo _copyRepo;
		private readonly ISubscribersRepo _subscribersRepo;
		private readonly IBookRepo _book;
		private readonly IMapper _mapper;

		public DashboardController(IBookCopyRepo copyRepo, ISubscribersRepo subscribersRepo, IMapper mapper, IBookRepo book)
		{
			_copyRepo = copyRepo;
			_subscribersRepo = subscribersRepo;
			_mapper = mapper;
			_book = book;
		}

		public async Task<IActionResult> Index()
		{


			var numberOfCopies = await _copyRepo.Count();
			var numberOfSubscribers = await _subscribersRepo.Count();
			var lastAddedBooks = await _book.LastAddedBooks();
			var topBooks = await _book.TopBooks();

			var model = new DashboardViewModel
			{
				NumberOfCopies = numberOfCopies,
				NumberOfSubscribers = numberOfSubscribers,
				LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks),
				TopBooks = _mapper.Map<IEnumerable<BookViewModel>>(topBooks)
			};


			return View(model);


		}
	}
}
