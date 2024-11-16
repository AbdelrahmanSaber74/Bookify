namespace Bookify.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IDataProtector _dataProtector;
        private readonly IBookRepo _bookRepo;
        private readonly IMapper _mapper;


		public SearchController(IDataProtectionProvider dataProtectionProvider, IBookRepo bookRepo, IMapper mapper)

		{
			_dataProtector = dataProtectionProvider.CreateProtector("MySecureKey");
			_bookRepo = bookRepo;
			_mapper = mapper;
		}

		public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(string bKey)
        {
            var bookId = int.Parse(_dataProtector.Unprotect(bKey));
            var book = await _bookRepo.GetBookByIdWithDetailsAsync(bookId);


            var viewModel = _mapper.Map<BookViewModel>(book);
          


            return View(viewModel);  

        }

		public async Task<IActionResult> Find(string query)
		{
            var books = await _bookRepo.FindBooks(query);

            foreach (var book in books)
            {
                book.Key = _dataProtector.Protect(book.Id.ToString() );
            }

			return Ok(books);

		}
	}
}
