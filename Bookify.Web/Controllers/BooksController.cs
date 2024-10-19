using System.Security.Claims;

namespace Bookify.Web.Controllers
{

    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly IBookRepo _bookRepo;
        private readonly IMapper _mapper;
        private readonly IAuthorRepo _authorRepo;
        private readonly ICategoriesRepo _categoriesRepo;
        private readonly IBookCategoryRepo _bookCategoryRepo;
        private readonly IBookCopyRepo _bookCopyRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;
        public BooksController(
            IBookRepo bookRepo,
            IMapper mapper,
            IAuthorRepo authorRepo,
            ICategoriesRepo categoriesRepo,
            IBookCategoryRepo bookCategoryRepo,
            IBookCopyRepo bookCopyRepo,
            IWebHostEnvironment webHostEnvironment,
            IImageService imageService)
        {
            _bookRepo = bookRepo;
            _mapper = mapper;
            _authorRepo = authorRepo;
            _categoriesRepo = categoriesRepo;
            _bookCategoryRepo = bookCategoryRepo;
            _bookCopyRepo = bookCopyRepo;
            _webHostEnvironment = webHostEnvironment;
            _imageService = imageService;
        }

        // Action method to display all books
        public async Task<IActionResult> Index()
        {
            var books = await _bookRepo.GetAllBooksIncludeAuthorAsync();
            var bookViewModels = _mapper.Map<IEnumerable<BookViewModel>>(books);
            return View(bookViewModels);
        }

        // Action method to display the create book view
        public async Task<IActionResult> Create()
        {
            var viewModel = await PopulateViewModelAsync();
            return View("AddBook", viewModel);
        }

        // POST method to add a new book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(BookViewModel model)
        {
            if (!ModelState.IsValid)
                return await ReturnAddBookViewWithErrorsAsync();

            var newBook = _mapper.Map<Book>(model);
            var lastBookId = await _bookRepo.GetLatestBookIdAsync();

            // Handle image upload
            if (model.Image != null)
            {
               var saveImageResult = await _imageService.SaveImageAsync(model.Image, "images/books");

                if (saveImageResult is OkObjectResult okResult)
                {
                    var resultData = (dynamic)okResult.Value;
                    newBook.ImageUrl = resultData.relativePath;
                    newBook.ImageThumbnailUrl = resultData.thumbnailRelativePath;
                }
                else
                {
                    return await ReturnAddBookViewWithErrorsAsync();
                }
            }

            await _bookRepo.AddBookAsync(newBook);
            newBook.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await AddBookCategoriesAsync(model.SelectedCategoryIds, newBook.Id);

            TempData["SuccessMessage"] = "Book added successfully!";


            return RedirectToAction(nameof(Details), new
            {
                id = newBook.Id,
            });


        }

        // Action method to display the edit book view
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookRepo.GetBookByIdAsync(id);
            if (book == null) return NotFound();

            var bookView = _mapper.Map<BookViewModel>(book);
            await PopulateSelectListsAsync(bookView);
            bookView.SelectedCategoryIds = await _bookCategoryRepo.GetCategoryIdsByBookIdAsync(book.Id);

            return View("EditBook", bookView);
        }

        // POST method to update an existing book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBook(BookViewModel model)

        {
            var existingBook = await _bookRepo.GetBookByIdAsync(model.Id);

            var bookCopies = await _bookCopyRepo.GetBookCopiesByBookIdAsync(model.Id);

            if (existingBook == null) return NotFound();


            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model);
                return View("EditBook", model);
            }


            _mapper.Map(model, existingBook);
            model.Copies = bookCopies;

            // Handle image update
            if (model.Image != null)
            {

                if (!string.IsNullOrEmpty(existingBook.ImageThumbnailUrl) || !string.IsNullOrEmpty(existingBook.ImageUrl))
                {
                    _imageService.DeleteOldImages(existingBook.ImageUrl, existingBook.ImageThumbnailUrl);
                }



                var saveImageResult = await _imageService.SaveImageAsync(model.Image, "images/books");
                if (saveImageResult is OkObjectResult okResult)
                {

                    var resultData = (dynamic)okResult.Value;

                    existingBook.ImageUrl = resultData.relativePath;
                    existingBook.ImageThumbnailUrl = resultData.thumbnailRelativePath;
                }
                else
                {
                    await PopulateSelectListsAsync(model);
                    ModelState.AddModelError(string.Empty, "Failed to save the image. Please try again.");
                    return View("EditBook", model);
                }
            }

            existingBook.LastUpdatedOn = DateTime.Now;
            existingBook.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (!model.IsAvailableForRental)
            {
                foreach (var copy in model.Copies)
                {
                    copy.IsAvailableForRental = false;
                    await _bookCopyRepo.UpdateBookCopyAsync(copy);
                }

            }

            await _bookRepo.UpdateBookAsync(existingBook);
            await UpdateBookCategoriesAsync(model.SelectedCategoryIds, existingBook.Id);

            TempData["SuccessMessage"] = "Book updated successfully!";

            return RedirectToAction(nameof(Details), new
            {
                id = existingBook.Id,
            });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existingBook = await _bookRepo.GetBookByIdAsync(id);

            if (existingBook == null)
            {
                return Json(new { success = false, message = "Book not found." });
            }


            if (!string.IsNullOrEmpty(existingBook.ImageThumbnailUrl) || !string.IsNullOrEmpty(existingBook.ImageUrl))
            {
                _imageService.DeleteOldImages(existingBook.ImageUrl, existingBook.ImageThumbnailUrl);
            }


            var categoryBookIds = await _bookCategoryRepo.GetCategoryIdsByBookIdAsync(id);

            if (categoryBookIds.Any())
            {
                foreach (var categoryId in categoryBookIds)
                {
                    var bookCatgory = await _bookCategoryRepo.GetBookCategoryByIdsAsync(id, categoryId);
                    await _bookCategoryRepo.RemoveAsync(bookCatgory);
                }
            }

            await _bookRepo.DeleteBookAsync(id);

            return Json(new { success = true, message = "Book deleted successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Fetch the book details
            var book = await _bookRepo.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }


            var bookView = _mapper.Map<BookViewModel>(book);

            // Fetch the selected category IDs and book copies concurrently
            bookView.SelectedCategoryIds = await _bookCategoryRepo.GetCategoryIdsByBookIdAsync(book.Id);
            // Fetch the author name
            var author = await _authorRepo.GetAuthorByIdAsync(book.AuthorId);
            bookView.AuthorName = author?.Name;
            bookView.Copies = await _bookCopyRepo.GetBookCopiesByBookIdAsync(id);


            foreach (var categoryId in bookView.SelectedCategoryIds)
            {
                var category = await _categoriesRepo.GetCategoryByIdAsync(categoryId);
                if (category != null) // Ensure category is not null
                {
                    bookView.NameOfCategories.Add(category.Name);
                }
            }

            return View(bookView);
        }



        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsTitleAuthorUnique(BookViewModel model)
        {
            var existingBook = await _bookRepo.GetBookByTitleAndAuthor(model.Title, model.AuthorId);

            // If the book is null, or it's the current book being updated, it's valid
            if (existingBook == null || existingBook.Id == model.Id)
            {
                return Json(true);
            }

            // Otherwise, return an error message indicating the combination is duplicated
            var errorMessage = string.Format(Errors.DuplicatedBook);
            return Json(errorMessage);


        }



        // Private methods for handling business logic

        // Populate view model for book creation
        private async Task<BookViewModel> PopulateViewModelAsync()
        {
            var authors = await _authorRepo.GetAvailableAuthorsAsync();
            var categories = await _categoriesRepo.GetAvailableCategoriesAsync();

            return new BookViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors).ToList(),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories).ToList()
            };
        }

        // Populate select lists for book editing
        private async Task PopulateSelectListsAsync(BookViewModel model)
        {
            model.Authors = _mapper.Map<IEnumerable<SelectListItem>>(await _authorRepo.GetAvailableAuthorsAsync()).ToList();
            model.Categories = _mapper.Map<IEnumerable<SelectListItem>>(await _categoriesRepo.GetAvailableCategoriesAsync()).ToList();
        }

        // Add categories to the book
        private async Task AddBookCategoriesAsync(IEnumerable<int> selectedCategoryIds, int bookId)
        {
            foreach (var categoryId in selectedCategoryIds)
            {
                var exists = await _bookCategoryRepo.GetBookCategoryByIdsAsync(bookId, categoryId);
                if (exists == null)
                {
                    var bookCategory = new BookCategory { CategoryId = categoryId, BookId = bookId };
                    await _bookCategoryRepo.AddAsync(bookCategory);
                }
            }
        }

        // Update book categories
        private async Task UpdateBookCategoriesAsync(IEnumerable<int> selectedCategoryIds, int bookId)
        {
            var existingBookCategories = await _bookCategoryRepo.GetCategoryIdsByBookIdAsync(bookId);

            foreach (var categoryId in existingBookCategories)
            {
                if (!selectedCategoryIds.Contains(categoryId))
                {
                    var bookCategoryToRemove = await _bookCategoryRepo.GetBookCategoryByIdsAsync(bookId, categoryId);
                    if (bookCategoryToRemove != null)
                    {
                        await _bookCategoryRepo.RemoveAsync(bookCategoryToRemove);
                    }
                }
            }

            await AddBookCategoriesAsync(selectedCategoryIds, bookId);
        }

        // Helper methods for error handling and file deletion

        private async Task<IActionResult> HandleImageErrorAsync(BookViewModel model, string error)
        {
            ModelState.AddModelError(nameof(model.Image), error);
            return await ReturnAddBookViewWithErrorsAsync();
        }

        private async Task<IActionResult> ReturnAddBookViewWithErrorsAsync()
        {
            var viewModel = await PopulateViewModelAsync();
            return View("AddBook", viewModel);
        }





    }
}
