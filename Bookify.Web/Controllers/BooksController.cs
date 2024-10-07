public class BooksController : Controller
{
    private readonly IBookRepo _bookRepo;
    private readonly IMapper _mapper;
    private readonly IAuthorRepo _authorRepo;
    private readonly ICategoriesRepo _categoriesRepo;
    private readonly IBookCategoryRepo _bookCategoryRepo;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Constructor for dependency injection
    public BooksController(
        IBookRepo bookRepo,
        IMapper mapper,
        IAuthorRepo authorRepo,
        ICategoriesRepo categoriesRepo,
        IBookCategoryRepo bookCategoryRepo,
        IWebHostEnvironment webHostEnvironment)
    {
        _bookRepo = bookRepo;
        _mapper = mapper;
        _authorRepo = authorRepo;
        _categoriesRepo = categoriesRepo;
        _bookCategoryRepo = bookCategoryRepo;
        _webHostEnvironment = webHostEnvironment;
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
            var saveImageResult = await SaveImageAsync(model.Image, model, lastBookId + 1);
            if (saveImageResult is OkObjectResult okResult)
            {
                newBook.ImageUrl = okResult.Value.ToString();
            }
            else
            {
                return await ReturnAddBookViewWithErrorsAsync();
            }
        }

        await _bookRepo.AddBookAsync(newBook);
        await AddBookCategoriesAsync(model.SelectedCategoryIds, newBook.Id);

        TempData["SuccessMessage"] = "Book added successfully!";
        return RedirectToAction(nameof(Index));
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
        if (existingBook == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(model);
            return View("EditBook", model);
        }

        _mapper.Map(model, existingBook);

        // Handle image update
        if (model.Image != null)
        {
            if (!string.IsNullOrEmpty(existingBook.ImageUrl))
            {
                DeleteOldImage(existingBook.ImageUrl);
            }

            var saveImageResult = await SaveImageAsync(model.Image, model, model.Id);
            if (saveImageResult is OkObjectResult okResult)
            {
                existingBook.ImageUrl = okResult.Value.ToString();
            }
            else
            {
                await PopulateSelectListsAsync(model);
                ModelState.AddModelError(string.Empty, "Failed to save the image. Please try again.");
                return View("EditBook", model);
            }
        }

        existingBook.LastUpdatedOn = DateTime.Now;
        await _bookRepo.UpdateBookAsync(existingBook);
        await UpdateBookCategoriesAsync(model.SelectedCategoryIds, existingBook.Id);

        TempData["SuccessMessage"] = "Book updated successfully!";
        return RedirectToAction(nameof(Index));
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

    // Handle image saving logic
    private async Task<IActionResult> SaveImageAsync(IFormFile file, BookViewModel model, int bookId)
    {
        if (file == null || file.Length == 0)
            return await HandleImageErrorAsync(model, Errors.NoFileProvided);

        const int maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
            return await HandleImageErrorAsync(model, string.Format(Errors.FileSizeExceeded, maxFileSize / (1024 * 1024)));

        var allowedFileTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (!allowedFileTypes.Contains(file.ContentType))
            return await HandleImageErrorAsync(model, string.Format(Errors.InvalidFileType, string.Join(", ", allowedFileTypes)));

        return await SaveFileToDiskAsync(file, bookId);
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

    private void DeleteOldImage(string imageUrl)
    {
        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
        if (System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }
    }

    private async Task<IActionResult> SaveFileToDiskAsync(IFormFile file, int bookId)
    {
        try
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            var imageName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books", bookId.ToString(), imageName);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("images", "books", bookId.ToString(), imageName).Replace("\\", "/");
            return Ok(relativePath);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(file), Errors.SaveError);
            return await ReturnAddBookViewWithErrorsAsync();
        }
    }
}
