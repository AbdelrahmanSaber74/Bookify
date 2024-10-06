using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

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
            if (model.Image != null)
            {
                var saveImageResult = await SaveImageAsync(model.Image, model, lastBookId + 1);

                // Check if the image was saved successfully
                if (saveImageResult is OkObjectResult okResult)
                {
                    newBook.ImageUrl = okResult.Value.ToString(); // Set the Image to the saved file path
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

        public async Task<IActionResult> Edit(int id)
        {
            // Retrieve the existing book using the provided ID
            var book = await _bookRepo.GetBookByIdAsync(id);

            if (book == null)
            {
                return NotFound();
            }


            //Map the book entity to the view model
            var bookView = _mapper.Map<BookViewModel>(book);

            bookView.Authors = _mapper.Map<IEnumerable<SelectListItem>>(await _authorRepo.GetAvailableAuthorsAsync()).ToList();
            bookView.Categories = _mapper.Map<IEnumerable<SelectListItem>>(await _categoriesRepo.GetAvailableCategoriesAsync()).ToList();
            bookView.SelectedCategoryIds = await _bookCategoryRepo.GetCategoryIdsByBookIdAsync(book.Id);


            return View("EditBook", bookView);

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBook(BookViewModel model)
        {
            // Fetch the existing book entity from the repository
            var existingBook = await _bookRepo.GetBookByIdAsync(model.Id);

            // If the book does not exist, return 404
            if (existingBook == null)
            {
                return NotFound();
            }

            // If the model is not valid, re-render the form with the necessary data
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model); // Populating the Authors and Categories lists
                return View("EditBook", model); // Return the invalid form with validation messages
            }


            // Map the updated values from the view model to the existing entity
            _mapper.Map(model, existingBook);

            // Handle the image upload if an image is provided  
            if (model.Image != null)
            {
                var saveImageResult = await SaveImageAsync(model.Image, model, model.Id);


                // Check if the image was saved successfully
                if (saveImageResult is OkObjectResult okResult)
                {
                    existingBook.ImageUrl = okResult.Value.ToString();
                }
                else
                {
                    await PopulateSelectListsAsync(model); // Ensure dropdowns are populated
                    ModelState.AddModelError(string.Empty, "Failed to save the image. Please try again.");
                    return View("EditBook", model);
                }
            }

            existingBook.LastUpdatedOn = DateTime.Now;

            // Persist the changes in the database
            await _bookRepo.UpdateBookAsync(existingBook);
            await UpdateBookCategoriesAsync(model.SelectedCategoryIds, existingBook.Id);

            // Store a success message in TempData and redirect to the Index page
            TempData["SuccessMessage"] = "Book updated successfully!";
            return RedirectToAction(nameof(Index));
        }




        // For Add
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


        //For Eidt
        private async Task<BookViewModel> PopulateSelectListsAsync(BookViewModel model)
        {
            // Fetch authors and categories from the repositories
            var authors = await _authorRepo.GetAvailableAuthorsAsync();
            var categories = await _categoriesRepo.GetAvailableCategoriesAsync();

            // Map the authors and categories to SelectListItems and assign to the model
            model.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors).ToList();
            model.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories).ToList();

            // Return the updated model
            return model;
        }



        private async Task<IActionResult> SaveImageAsync(IFormFile file, BookViewModel model, int bookId)
        {

            // Check if a file has been provided
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Image), Errors.NoFileProvided);
                return await ReturnAddBookViewWithErrorsAsync();
            }

            // Define maximum file size (e.g., 5 MB)
            const int maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxFileSize)
            {
                ModelState.AddModelError(nameof(model.Image), string.Format(Errors.FileSizeExceeded, maxFileSize / (1024 * 1024)));
                return await ReturnAddBookViewWithErrorsAsync();
            }

            // Define allowed file types
            var allowedFileTypes = new[] { "image/jpeg", "image/png", "image/gif" }; // Allowed file types
            if (!allowedFileTypes.Contains(file.ContentType))
            {
                ModelState.AddModelError(nameof(model.Image), string.Format(Errors.InvalidFileType, string.Join(", ", allowedFileTypes)));
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
                ModelState.AddModelError(nameof(model.Image), Errors.PermissionDenied);
            }
            catch (Exception)
            {
                ModelState.AddModelError(nameof(model.Image), Errors.SaveError);
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
                // Check if the relationship already exists
                var existing = await _bookCategoryRepo.GetBookCategoryByIdsAsync(bookId, categoryId);
                if (existing == null)
                {
                    var bookCategory = new BookCategory
                    {
                        CategoryId = categoryId,
                        BookId = bookId
                    };
                    await _bookCategoryRepo.AddAsync(bookCategory);
                }
            }
        }


        private async Task UpdateBookCategoriesAsync(List<int> selectedCategoryIds, int bookId)
        {
            // Retrieve all existing BookCategories for the given bookId
            var existingBookCategories = await _bookCategoryRepo.GetCategoryIdsByBookIdAsync(bookId);

            // Remove categories that are no longer selected
            foreach (var categoryId in existingBookCategories)
            {
                if (!selectedCategoryIds.Contains(categoryId))
                {
                    // Assuming you have a method to get the full BookCategory by categoryId and bookId
                    var bookCategoryToRemove = await _bookCategoryRepo.GetBookCategoryByIdsAsync(bookId, categoryId);
                    if (bookCategoryToRemove != null)
                    {
                        await _bookCategoryRepo.RemoveAsync(bookCategoryToRemove);
                    }
                }
            }



            await AddBookCategoriesAsync(selectedCategoryIds, bookId);

        }



        //[HttpGet]
        //public async Task<IActionResult> ValidateImage(IFormFile? Image)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //        // Log errors for debugging
        //    }

        //    // Check if a file has been provided
        //    if (Image == null || Image.Length == 0)
        //    {
        //        return Json(new { success = false, message = Errors.NoFileProvided });
        //    }

        //    // Define maximum file size (e.g., 5 MB)
        //    const int maxFileSize = 5 * 1024 * 1024; // 5 MB
        //    if (Image.Length > maxFileSize)
        //    {
        //        return Json(new { success = false, message = string.Format(Errors.FileSizeExceeded, maxFileSize / (1024 * 1024)) });
        //    }

        //    // Define allowed file types
        //    var allowedFileTypes = new[] { "image/jpeg", "image/png", "image/gif" }; // Allowed file types
        //    if (!allowedFileTypes.Contains(Image.ContentType))
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
