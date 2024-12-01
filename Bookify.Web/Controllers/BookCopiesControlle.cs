namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly IBookCopyRepo _copyRepo;
        private readonly IBookRepo _bookRepo;
        private readonly IRentalCopyRepo _rentalCopyRepo;
        private readonly IMapper _mapper;

        public BookCopiesController(IBookCopyRepo copyRepo, IMapper mapper, IBookRepo bookRepo, IRentalCopyRepo rentalCopyRepo)
        {
            _copyRepo = copyRepo;
            _mapper = mapper;
            _bookRepo = bookRepo;
            _rentalCopyRepo = rentalCopyRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int bookId)
        {
            var book = await _bookRepo.GetBookByIdAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            var bookCopyView = _mapper.Map<BookCopyViewModel>(book);
            return View("AddBookCopies", bookCopyView);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var bookCopy = await _copyRepo.GetBookCopyByIdAsync(id);
            if (bookCopy == null)
            {
                return NotFound();
            }

            var bookCopyView = _mapper.Map<BookCopyViewModel>(bookCopy);
            return View("EditBookCopies", bookCopyView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBookCopies(BookCopyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await GenerateBookCopiesViewWithErrorsAsync(model, "AddBookCopies");
            }

            var bookCopy = _mapper.Map<BookCopy>(model);
            bookCopy.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _copyRepo.AddBookCopyAsync(bookCopy);
            TempData["SuccessMessage"] = "Book Copies added successfully.";

            return RedirectToAction("Details", "Books", new { id = model.BookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBookCopies(BookCopyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await GenerateBookCopiesViewWithErrorsAsync(model, "EditBookCopies");
            }

            var bookCopy = _mapper.Map<BookCopy>(model);
            bookCopy.LastUpdatedOn = DateTime.Now;
            bookCopy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _copyRepo.UpdateBookCopyAsync(bookCopy);
            TempData["SuccessMessage"] = "Book Copies updated successfully.";

            return RedirectToAction("Details", "Books", new { id = bookCopy.BookId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var bookCopy = await _copyRepo.GetBookCopyByIdAsync(id);
            if (bookCopy == null)
            {
                return NotFound();
            }

            await _copyRepo.DeleteBookCopyByIdAsync(id);
            return Json(new
            {
                success = true,
                message = "Book copy deleted successfully"
            });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var bookCopy = await _copyRepo.GetBookCopyByIdAsync(id);
            if (bookCopy == null)
            {
                return NotFound();
            }

            // Toggle the IsDeleted property
            bookCopy.IsDeleted = !bookCopy.IsDeleted;
            bookCopy.LastUpdatedOn = DateTime.Now;
            bookCopy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Update the book copy in the repository
            await _copyRepo.UpdateBookCopyAsync(bookCopy);

            return Json(new
            {
                success = true,
                lastUpdatedOn = bookCopy.LastUpdatedOn.ToString()
            });
        }


        [HttpGet]
        public async Task<IActionResult> RentalHistory(int id)
        {

            var bookCopy = await _rentalCopyRepo.GetRentalHistoryAsync(id);

            if (bookCopy == null) return NotFound();

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(bookCopy);

            return View(viewModel);

        }
        // Method Helper 
        private async Task<IActionResult> GenerateBookCopiesViewWithErrorsAsync(BookCopyViewModel? model, string addView)
        {
            model ??= new BookCopyViewModel();
            var books = await _bookRepo.GetAllBooksAsync();

            model.Books = books.Select(b => new SelectListItem
            {
                Text = b.Title,
                Value = b.Id.ToString()
            }).ToList();

            return View(addView, model);
        }
    }
}
