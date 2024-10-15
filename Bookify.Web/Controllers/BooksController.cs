using Microsoft.AspNetCore.Authorization;

[Authorize]
public class BooksController : Controller
{
    private readonly IBookRepo _bookRepo;
    private readonly IMapper _mapper;
    private readonly IAuthorRepo _authorRepo;
    private readonly ICategoriesRepo _categoriesRepo;
    private readonly IBookCategoryRepo _bookCategoryRepo;
    private readonly IBookCopyRepo _bookCopyRepo;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Constructor for dependency injection
    public BooksController(
        IBookRepo bookRepo,
        IMapper mapper,
        IAuthorRepo authorRepo,
        ICategoriesRepo categoriesRepo,
        IBookCategoryRepo bookCategoryRepo,
        IBookCopyRepo bookCopyRepo,
        IWebHostEnvironment webHostEnvironment)
    {
        _bookRepo = bookRepo;
        _mapper = mapper;
        _authorRepo = authorRepo;
        _categoriesRepo = categoriesRepo;
        _bookCategoryRepo = bookCategoryRepo;
        _bookCopyRepo = bookCopyRepo;
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

            if (!string.IsNullOrEmpty(existingBook.ImageThumbnailUrl) || !string.IsNullOrEmpty(existingBook.ImageUrl))
            {
                DeleteOldImages(existingBook.ImageUrl, existingBook.ImageThumbnailUrl);
            }



            var saveImageResult = await SaveImageAsync(model.Image, model, model.Id);
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
            DeleteOldImages(existingBook.ImageUrl, existingBook.ImageThumbnailUrl);
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

    private void DeleteOldImages(string imageUrl, string thumbnailUrl)
    {
        // Check if imageUrl is null or empty
        if (string.IsNullOrEmpty(imageUrl) && string.IsNullOrEmpty(thumbnailUrl))
        {
            return; // Nothing to delete
        }

        // Delete the old image
        if (!string.IsNullOrEmpty(imageUrl))
        {
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl);
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
        }

        // Delete the old thumbnail
        if (!string.IsNullOrEmpty(thumbnailUrl))
        {
            var oldThumbnailPath = Path.Combine(_webHostEnvironment.WebRootPath, thumbnailUrl);
            if (System.IO.File.Exists(oldThumbnailPath))
            {
                System.IO.File.Delete(oldThumbnailPath);
            }
        }
    }



    private async Task<IActionResult> SaveFileToDiskAsync(IFormFile file, int bookId)
    {
        try
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            var imageName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books", imageName);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Resize and create a thumbnail
            var thumbnailPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books", "thumb", imageName);

            var thumbnailDirectory = Path.GetDirectoryName(thumbnailPath);
            if (!Directory.Exists(thumbnailDirectory))
            {
                Directory.CreateDirectory(thumbnailDirectory);
            }

            // Resize the original image and save it as a thumbnail (150 width and height auto genrate)
            ImageHelper.ResizeImage(filePath, thumbnailPath, width: 150);


            var relativePath = "/" + Path.Combine("images", "books", imageName).Replace("\\", "/");
            var thumbnailRelativePath = "/" + Path.Combine("images", "books", "thumb", imageName).Replace("\\", "/");

            return Ok(new
            {
                relativePath,
                thumbnailRelativePath
            });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(file), Errors.SaveError);
            return await ReturnAddBookViewWithErrorsAsync();
        }
    }


    public IActionResult IndexPlus()
    {
        return View();
    }



    // the functionality of sending data to the server using server-side processing for DataTables.
    [HttpPost]
    public async Task<IActionResult> GetBooksData()
    {
        // Read parameters sent by DataTable
        var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
        var start = HttpContext.Request.Form["start"].FirstOrDefault();
        var length = HttpContext.Request.Form["length"].FirstOrDefault();
        var sortColumnIndex = HttpContext.Request.Form["order[0][column]"].FirstOrDefault();
        var sortDirection = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = HttpContext.Request.Form["search[value]"].FirstOrDefault();

        // Convert to int
        int pageSize = length != null ? Convert.ToInt32(length) : 0;
        int skip = start != null ? Convert.ToInt32(start) : 0;

        // Fetch data from the database (queryable)
        var bookQuery = await _bookRepo.GetAllBooksAsQueryableAsync();

        // Apply search filter if needed
        if (!string.IsNullOrEmpty(searchValue))
        {
            bookQuery = bookQuery.Where(b => b.Title.Contains(searchValue) || b.Author.Name.Contains(searchValue));
        }

        // Get total record count before pagination and filtering
        var totalRecords = await bookQuery.CountAsync();

        // Apply sorting based on the column index
        switch (sortColumnIndex)
        {
            case "0":
                bookQuery = sortDirection == "asc" ? bookQuery.OrderBy(b => b.Title) : bookQuery.OrderByDescending(b => b.Title);
                break;
            case "1":
                bookQuery = sortDirection == "asc" ? bookQuery.OrderBy(b => b.Author.Name) : bookQuery.OrderByDescending(b => b.Author.Name);
                break;
            case "2":
                bookQuery = sortDirection == "asc" ? bookQuery.OrderBy(b => b.Publisher) : bookQuery.OrderByDescending(b => b.Publisher);
                break;
            case "3":
                bookQuery = sortDirection == "asc" ? bookQuery.OrderBy(b => b.IsDeleted) : bookQuery.OrderByDescending(b => b.IsDeleted);
                break;
            case "4":
                bookQuery = sortDirection == "asc" ? bookQuery.OrderBy(b => b.CreatedOn) : bookQuery.OrderByDescending(b => b.CreatedOn);
                break;
            case "5":
                bookQuery = sortDirection == "asc" ? bookQuery.OrderBy(b => b.LastUpdatedOn) : bookQuery.OrderByDescending(b => b.LastUpdatedOn);
                break;
            default:
                bookQuery = bookQuery.OrderBy(b => b.Title); // Default sorting by Title
                break;
        }

        // Paginate the result
        var paginatedBooks = await bookQuery.Skip(skip).Take(pageSize)
            .Select(b => new
            {
                b.Id,
                b.Title,
                AuthorName = b.Author.Name,
                b.Publisher,
                b.IsDeleted,
                b.CreatedOn,
                LastUpdated = b.LastUpdatedOn
            })
            .ToListAsync();

        // Return the result in JSON format with draw and count information
        var jsonData = new
        {
            draw,
            recordsFiltered = totalRecords, // Total records after filtering
            recordsTotal = totalRecords,    // Total records without filtering
            data = paginatedBooks
        };

        return Json(jsonData);
    }





}
