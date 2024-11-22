using Bookify.Web.Core.Utilities;
using ClosedXML.Excel;


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


        public async Task<IActionResult> ExportBooksToExcel(IList<int>? Authors, IList<int>? categories)
        {
            var books = (await _bookRepo.GetBooksWithDetailsAsync()).AsQueryable();

            // Filter books by selected authors if provided
            if (Authors != null && Authors.Any())
            {
                books = books.Where(b => Authors.Contains(b.AuthorId));
            }

            // Filter books by selected categories if provided
            if (categories != null && categories.Any())
            {
                books = books.Where(b => b.Categories.Any(c => categories.Contains(c.CategoryId)));
            }

            // Convert to list for iteration
            var filteredBooks = books.ToList();

            using var workBook = new XLWorkbook();
            var sheet = workBook.AddWorksheet("Books");


            // Set header row with formatting
            var headerRow = sheet.Row(1);
            headerRow.Style.Font.Bold = true; // Make the font bold
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray; // Set background color
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Center alignment


            var headers = new[]
                    {
                        "Title", "Author", "Categories", "Publisher",
                        "Publishing Date", "Hall", "Available for Rental", "Status"
                    };

            for (int col = 0; col < headers.Length; col++)
            {
                sheet.Cell(1, col + 1).SetValue(headers[col]);
            }


            // Populate rows with book data
            for (int i = 0; i < filteredBooks.Count; i++)
            {
                var book = filteredBooks[i];
                sheet.Cell(i + 2, 1).SetValue(book.Title);
                sheet.Cell(i + 2, 2).SetValue(book.Author?.Name ?? "Unknown");
                sheet.Cell(i + 2, 3).SetValue(string.Join(", ", book.Categories.Select(c => c.Category.Name)));
                sheet.Cell(i + 2, 4).SetValue(book.Publisher ?? "Unknown");
                sheet.Cell(i + 2, 5).SetValue(book.PublishingDate.ToShortDateString() ?? "N/A");
                sheet.Cell(i + 2, 6).SetValue(book.Hall ?? "N/A");
                sheet.Cell(i + 2, 7).SetValue(book.IsAvailableForRental ? "Yes" : "No");
                sheet.Cell(i + 2, 8).SetValue(book.IsDeleted ? "Deleted" : "Available");
            }

            sheet.ColumnsUsed().AdjustToContents();

            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.CellsUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.CellsUsed().Style.Border.OutsideBorderColor = XLColor.Black;


            // Save the workbook to a memory stream
            var stream = new MemoryStream();
            workBook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Return the file as a downloadable response
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "Books.xlsx";
            return File(stream, contentType, fileName);
        }


    }
}
