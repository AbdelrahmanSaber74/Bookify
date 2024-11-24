using Bookify.Web.Core.Utilities;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenHtmlToPdf;
using ViewToHTML.Services;


namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ReportsController : Controller
    {
        private readonly IBookRepo _bookRepo;
        private readonly IRentalRepo _rentalRepo;
        private readonly IRentalCopyRepo _rentalCopyRepo;
        private readonly IAuthorRepo _authorRepo;
        private readonly ICategoriesRepo _categoriesRepo;
        private readonly IMapper _mapper;
        private readonly IViewRendererService _viewRendererService;

        public ReportsController(IBookRepo bookRepo, IAuthorRepo authorRepo, ICategoriesRepo categoriesRepo, IMapper mapper, IViewRendererService viewRendererService, IRentalCopyRepo rentalCopyRepo, IRentalRepo rentalRepo)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _categoriesRepo = categoriesRepo;
            _mapper = mapper;
            _viewRendererService = viewRendererService;
            _rentalCopyRepo = rentalCopyRepo;
            _rentalRepo = rentalRepo;
        }

        // Index Action
        public IActionResult Index()
        {
            return View();
        }


        #region Books

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


            var headerCells = new[]
                    {
                        "Title", "Author", "Categories", "Publisher",
                        "Publishing Date", "Hall", "Available for Rental", "Status"
                    };

            sheet.AddHeader(headerCells);

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

            sheet.Format();

            // Save the workbook to a memory stream
            var stream = new MemoryStream();
            workBook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Return the file as a downloadable response
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "Books.xlsx";
            return File(stream, contentType, fileName);
        }


        public async Task<IActionResult> ExportBooksToPDF(IList<int>? Authors, IList<int>? categories)
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
            var filteredBooks = _mapper.Map<IEnumerable<BookViewModel>>(books.ToList());

            var templetePAth = "~/Views/Reports/BooksTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templetePAth, filteredBooks);

            var pdf = Pdf.From(html).Content();

            // Return the file as a downloadable response
            var contentType = "application/pdf";
            var fileName = "Books.pdf";
            return File(pdf, contentType, fileName);
        }

        #endregion

        #region Rentals

        [HttpGet]
        public async Task<IActionResult> Rentals(int? pageNumber, string? duration)
        {
            var rentalCopies = (await _rentalCopyRepo.GetAllAsync()).AsQueryable();

            var model = new RentalsReportViewModel
            {
                Duration = duration
            };

            if (!string.IsNullOrEmpty(duration))
            {
                var dateRange = duration.Split(" - ");

                if (dateRange.Length == 2)
                {
                    if (!DateTime.TryParse(dateRange[0], out DateTime startDate))
                    {
                        ModelState.AddModelError("Duration", Errors.InvalidStartDate);
                        return View(model);
                    }

                    if (!DateTime.TryParse(dateRange[1], out DateTime endDate))
                    {
                        ModelState.AddModelError("Duration", Errors.InvalidEndDate);
                        return View(model);
                    }

                    rentalCopies = rentalCopies.Where(copy => copy.RentalDate >= startDate && copy.RentalDate <= endDate)
                                               .OrderBy(r => r.RentalDate);
                }
                else
                {
                    // Invalid duration format, add a model state error
                    ModelState.AddModelError("Duration", "The date range format is invalid.");
                    return View(model);
                }
            }


            if (pageNumber.HasValue)
            {
                model.Rentals = PaginatedList<RentalCopy>.Create(rentalCopies, pageNumber.Value, (int)ReportsConfigurations.PageSize);
            }

            return View(model);
        }



        public async Task<IActionResult> ExportRentalsToExcel(string? duration)
        {
            var rentalCopies = (await _rentalCopyRepo.GetAllAsync()).AsQueryable();

            if (!string.IsNullOrEmpty(duration))
            {
                var dateRange = duration.Split(" - ");

                if (dateRange.Length == 2)
                {
                    if (!DateTime.TryParse(dateRange[0], out DateTime startDate))
                    {
                        return BadRequest("Invalid start date.");
                    }

                    if (!DateTime.TryParse(dateRange[1], out DateTime endDate))
                    {
                        return BadRequest("Invalid end date.");
                    }

                    rentalCopies = rentalCopies.Where(copy => copy.RentalDate >= startDate && copy.RentalDate <= endDate)
                                               .OrderBy(r => r.RentalDate);
                }
                else
                {
                    return BadRequest("Invalid duration format.");
                }
            }

            // Convert the filtered rental copies to a list
            var filteredRentals = rentalCopies.ToList();

            using var workBook = new XLWorkbook();
            var sheet = workBook.AddWorksheet("Rentals");

            // Set header row with formatting
            var headerRow = sheet.Row(1);
            headerRow.Style.Font.Bold = true; // Make the font bold
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray; // Set background color
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Center alignment

            // Define header columns
            var headerCells = new[]
            {
            "Subscriber ID", "Subscriber Name", "Mobile Number", "Book Title",
            "Author", "Serial Number", "Rental Date", "End Date", "Return Date", "Extended On"
            };

            // Add the header cells to the sheet
            for (int i = 0; i < headerCells.Length; i++)
            {
                sheet.Cell(1, i + 1).SetValue(headerCells[i]);
            }

            // Populate rows with rental data
            for (int i = 0; i < filteredRentals.Count; i++)
            {
                var rental = filteredRentals[i];
                var subscriber = rental.Rental?.Subscriber;
                var bookCopy = rental.BookCopy;

                sheet.Cell(i + 2, 1).SetValue(rental.Rental?.Subscriber?.Id.ToString() ?? "N/A");
                sheet.Cell(i + 2, 2).SetValue($"{subscriber?.FirstName} {subscriber?.LastName}" ?? "N/A");
                sheet.Cell(i + 2, 3).SetValue(subscriber?.MobileNumber ?? "N/A");
                sheet.Cell(i + 2, 4).SetValue(bookCopy?.Book?.Title ?? "N/A");
                sheet.Cell(i + 2, 5).SetValue(bookCopy?.Book?.Author?.Name ?? "N/A");
                sheet.Cell(i + 2, 6).SetValue(bookCopy?.SerialNumber.ToString() ?? "N/A"); 
                sheet.Cell(i + 2, 7).SetValue(rental.RentalDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + 2, 8).SetValue(rental.EndDate.ToString("d MMM, yyyy") ?? "N/A");
                sheet.Cell(i + 2, 9).SetValue(rental.ReturnDate?.ToString("d MMM, yyyy") ?? "N/A");
                sheet.Cell(i + 2, 10).SetValue(rental.ExtendedOn?.ToString("d MMM, yyyy") ?? "N/A");
            }

            // Apply formatting to the sheet
            sheet.Columns().AdjustToContents();

            // Save the workbook to a memory stream
            var stream = new MemoryStream();
            workBook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Return the file as a downloadable response
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "RentalsReport.xlsx";
            return File(stream, contentType, fileName);
        }



        public async Task<IActionResult> ExportRentalsToPDF(string? duration)
        {

            var rentalCopies = (await _rentalCopyRepo.GetAllAsync()).AsQueryable();

            if (!string.IsNullOrEmpty(duration))
            {
                var dateRange = duration.Split(" - ");

                if (dateRange.Length == 2)
                {
                    if (!DateTime.TryParse(dateRange[0], out DateTime startDate))
                    {
                        return BadRequest("Invalid start date.");
                    }

                    if (!DateTime.TryParse(dateRange[1], out DateTime endDate))
                    {
                        return BadRequest("Invalid end date.");
                    }

                    rentalCopies = rentalCopies.Where(copy => copy.RentalDate >= startDate && copy.RentalDate <= endDate)
                                               .OrderBy(r => r.RentalDate);
                }
                else
                {
                    return BadRequest("Invalid duration format.");
                }
            }


            // Convert to list for iteration
            var filtereRentals = _mapper.Map<IEnumerable<RentalCopy>>(rentalCopies.ToList());

            var templetePAth = "~/Views/Reports/RentalCopiesTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templetePAth, filtereRentals);

            var pdf = Pdf.From(html).Content();

            // Return the file as a downloadable response
            var contentType = "application/pdf";
            var fileName = "Rentals.pdf";
            return File(pdf, contentType, fileName);
        }


        #endregion

        #region DelayedRentals

        [HttpGet]
        public async Task<IActionResult> DelayedRentals()
        {
            var delayedRentals = await _rentalRepo.GetDelayedRentalsAsync();
            var model = _mapper.Map<IEnumerable<RentalCopyViewModel>>(delayedRentals);
            
            return View(model);
        }

        public async Task<IActionResult> ExportDelayedRentalsToExcel()
        {
            // Fetch delayed rentals from the repository
            var delayedRentals = await _rentalRepo.GetDelayedRentalsAsync();
            var rentalViewModels = _mapper.Map<IEnumerable<RentalCopyViewModel>>(delayedRentals);

            // Create an Excel workbook and worksheet
            using var workBook = new XLWorkbook();
            var sheet = workBook.AddWorksheet("DelayedRentals");

            // Set header row style and alignment
            var headerRow = sheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Define the headers for the Excel file
            var headerCells = new[]
            {
                "Subscriber ID", "Subscriber Name", "Subscriber Mobile",
                "Book Title", "Serial Number", "Rental Date",
                "End Date", "Extended On", "Delay in Days"
            };

            // Add headers to the worksheet
            for (int i = 0; i < headerCells.Length; i++)
            {
                sheet.Cell(1, i + 1).Value = headerCells[i];
            }

            // Populate the sheet with rental data
            for (int i = 0; i < rentalViewModels.Count(); i++)
            {
                var rental = rentalViewModels.ElementAt(i);

                // Fill in the data for each rental row
                sheet.Cell(i + 2, 1).Value = rental.Rental!.Subscriber!.Id;
                sheet.Cell(i + 2, 2).Value = rental.Rental.Subscriber!.FullName;
                sheet.Cell(i + 2, 3).Value = rental.Rental.Subscriber!.MobileNumber;
                sheet.Cell(i + 2, 4).Value = rental.BookCopy!.BookTitle;
                sheet.Cell(i + 2, 5).Value = rental.BookCopy.SerialNumber;
                sheet.Cell(i + 2, 6).Value = rental.RentalDate.ToString("d MMM, yyyy");
                sheet.Cell(i + 2, 7).Value = rental.EndDate.ToString("d MMM, yyyy");
                sheet.Cell(i + 2, 8).Value = rental.ExtendedOn?.ToString("d MMM, yyyy");
                sheet.Cell(i + 2, 9).Value = rental.DelayInDays;
            }

            // Adjust column widths for readability
            sheet.Columns().AdjustToContents();

            // Save the workbook to a memory stream
            var stream = new MemoryStream();
            workBook.SaveAs(stream);

            // Reset the stream position to the beginning
            stream.Seek(0, SeekOrigin.Begin);

            // Return the file as a downloadable response
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "DelayedRentals.xlsx";

            // Do not dispose the stream until after it is written to the response
            return File(stream, contentType, fileName);
        }


        public async Task<IActionResult> ExportDelayedRentalsToPDF(IList<int>? Authors, IList<int>? categories)
        {
            var delayedRentals = await _rentalRepo.GetDelayedRentalsAsync();
            var rentalViewModels = _mapper.Map<IEnumerable<RentalCopyViewModel>>(delayedRentals);

            var templatePath = "~/Views/Reports/DelayedRentalsTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, rentalViewModels);

            var pdf = Pdf.From(html).Content();

            // Return the file as a downloadable response
            var contentType = "application/pdf";
            var fileName = "Books.pdf";
            return File(pdf, contentType, fileName);
        }






        #endregion

    }
}
