using Microsoft.AspNetCore.Hosting;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookCategoryRepo _bookCategoryRepo;
        private readonly ICategoriesRepo _categoriesRepo;
        private readonly IAuthorRepo _authorRepo;
        private readonly IBookRepo _bookRepo;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public BooksController(IBookRepo bookRepo,
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


        public async Task<IActionResult> Index()
        {
            var books = await _bookRepo.GetAllBooksIncludeAuthorAsync();
            var bookViewModels = _mapper.Map<IEnumerable<BookViewModel>>(books);
            return View(bookViewModels);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = await PopulateViewModelAsync();
            return View("AddBook", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(BookViewModel model)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return await ReturnAddBookViewWithErrorsAsync();
            }

            // Map the view model to a new Book entity
            var newBook = _mapper.Map<Book>(model);

            int lastBookId = await _bookRepo.GetLatestBookIdAsync();

            // Handle the image upload if provided
            if (model.ImageUrl != null)
            {
                var saveImageResult = await SaveImageAsync(model.ImageUrl, model, lastBookId + 1);

                // Check if the image was saved successfully
                if (saveImageResult is OkObjectResult okResult)
                {
                    newBook.ImageUrl = okResult.Value.ToString(); // Set the ImageUrl to the saved file path
                }
                else
                {
                    return await ReturnAddBookViewWithErrorsAsync();
                }
            }

            // Save the new book to the database
            await _bookRepo.AddBookAsync(newBook);

            // Add selected categories to the book
            await AddBookCategoriesAsync(model.SelectedCategoryIds, newBook.Id);

            TempData["SuccessMessage"] = "Book added successfully!";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await PopulateViewModelAsync();
                return View(viewModel);
            }

            // Save logic here

            return Json(model);
        }

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


        public async Task<IActionResult> SaveImageAsync(IFormFile file, BookViewModel model, int bookId)
        {

            // Check if a file has been provided
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(nameof(model.ImageUrl), Errors.NoFileProvided);
                return await ReturnAddBookViewWithErrorsAsync();
            }

            // Define maximum file size (e.g., 5 MB)
            const int maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxFileSize)
            {
                ModelState.AddModelError(nameof(model.ImageUrl), string.Format(Errors.FileSizeExceeded, maxFileSize / (1024 * 1024)));
                return await ReturnAddBookViewWithErrorsAsync();
            }

            // Define allowed file types
            var allowedFileTypes = new[] { "image/jpeg", "image/png", "image/gif" }; // Allowed file types
            if (!allowedFileTypes.Contains(file.ContentType))
            {
                ModelState.AddModelError(nameof(model.ImageUrl), string.Format(Errors.InvalidFileType, string.Join(", ", allowedFileTypes)));
                return await ReturnAddBookViewWithErrorsAsync();
            }

            try
            {

                // Define the path to save the file
                var extension = Path.GetExtension(file.FileName).ToLower();
                string imageName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books", bookId.ToString(), imageName);

                // Create the uploads directory if it does not exist
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }


                // Save the relative path to the database
                var relativePath = Path.Combine("images", "books", bookId.ToString(), imageName).Replace("\\", "/");
                return Ok(relativePath); // Return success with the file path


            }
            catch (UnauthorizedAccessException)
            {
                ModelState.AddModelError(nameof(model.ImageUrl), Errors.PermissionDenied);
            }
            catch (Exception)
            {
                ModelState.AddModelError(nameof(model.ImageUrl), Errors.SaveError);
            }

            return await ReturnAddBookViewWithErrorsAsync();
        }


        // Returns the AddBook view with validation errors
        private async Task<IActionResult> ReturnAddBookViewWithErrorsAsync()
        {
            var viewModel = await PopulateViewModelAsync();
            return View("AddBook", viewModel);
        }

        // Adds categories to the book
        private async Task AddBookCategoriesAsync(IEnumerable<int> selectedCategoryIds, int bookId)
        {
            foreach (var categoryId in selectedCategoryIds)
            {
                var bookCategory = new BookCategory
                {
                    CategoryId = categoryId,
                    BookId = bookId
                };
                await _bookCategoryRepo.AddAsync(bookCategory);
            }
        }


        //[HttpGet]
        //public async Task<IActionResult> ValidateImage(IFormFile? imageUrl)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //        // Log errors for debugging
        //    }

        //    // Check if a file has been provided
        //    if (imageUrl == null || imageUrl.Length == 0)
        //    {
        //        return Json(new { success = false, message = Errors.NoFileProvided });
        //    }

        //    // Define maximum file size (e.g., 5 MB)
        //    const int maxFileSize = 5 * 1024 * 1024; // 5 MB
        //    if (imageUrl.Length > maxFileSize)
        //    {
        //        return Json(new { success = false, message = string.Format(Errors.FileSizeExceeded, maxFileSize / (1024 * 1024)) });
        //    }

        //    // Define allowed file types
        //    var allowedFileTypes = new[] { "image/jpeg", "image/png", "image/gif" }; // Allowed file types
        //    if (!allowedFileTypes.Contains(imageUrl.ContentType))
        //    {
        //        return Json(new { success = false, message = string.Format(Errors.InvalidFileType, string.Join(", ", allowedFileTypes)) });
        //    }

        //    try
        //    {
        //        // If you need to perform additional validation (like checking for duplicate images, etc.), do that here

        //        return Json(new { success = true }); // Validation passed
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Json(new { success = false, message = Errors.PermissionDenied });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { success = false, message = Errors.SaveError });
        //    }
        //}







    }
}
