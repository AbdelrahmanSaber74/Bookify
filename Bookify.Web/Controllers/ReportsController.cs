using Bookify.Web.Core.Utilities;


namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ReportsController : Controller
    {
        private readonly IBookRepo _bookRepo;
        private readonly IAuthorRepo _authorRepo;
        private readonly ICategoriesRepo _categoriesRepo;
        private readonly IMapper _mapper;

        public ReportsController(IBookRepo bookRepo, IAuthorRepo authorRepo, ICategoriesRepo categoriesRepo, IMapper mapper)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _categoriesRepo = categoriesRepo;
            _mapper = mapper;
        }

        // Index Action
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Books(IList<int> SelectedAuthors, IList<int> SelectedCategories, int? pageNumber)
        {
            var books = (await _bookRepo.GetBooksWithDetailsAsync()).AsQueryable();
            var categories = await _categoriesRepo.GetAllCategoriesAsync();
            var authors = await _authorRepo.GetAllAuthorsAsync();

            // Filter books by selected authors if any
            if (SelectedAuthors != null && SelectedAuthors.Any())
            {
                books = books.Where(b => SelectedAuthors.Contains(b.AuthorId));
            }

            // Filter books by selected categories if any
            if (SelectedCategories != null && SelectedCategories.Any())
            {
                books = books.Where(b => b.Categories.Any(c => SelectedCategories.Contains(c.CategoryId)));
            }

            var model = new BooksReportViewModel
            {
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories),
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
            };

            if (pageNumber.HasValue)
            {
                model.Books = PaginatedList<Book>.Create(books, pageNumber.Value, (int)ReportsConfigurations.PageSize);
            }

            return View(model);
        }


    }
}
