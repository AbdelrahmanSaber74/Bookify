using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Repositories.Authors;
using Bookify.Web.Repositories.Books;
using Bookify.Web.Repositories.BooksCategories;
using Bookify.Web.Repositories.Categories;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Manage.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Query;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookCategoryRepo _bookCategoryRepo;
        private readonly ICategoriesRepo _categoriesRepo;
        private readonly IAuthorRepo _authorRepo;
        private readonly IBookRepo _bookRepo;
        private readonly IMapper _mapper;

        public BooksController(IBookRepo bookRepo,
                               IMapper mapper,
                               IAuthorRepo authorRepo,
                               ICategoriesRepo categoriesRepo,
                               IBookCategoryRepo bookCategoryRepo) 
        {
            _bookRepo = bookRepo;
            _mapper = mapper;
            _authorRepo = authorRepo;
            _categoriesRepo = categoriesRepo;
            _bookCategoryRepo = bookCategoryRepo; 
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
        public async Task<IActionResult> AddBook(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await PopulateViewModelAsync();
                return View("AddBook", viewModel);
            }

            var newBook = _mapper.Map<Book>(model);
            await _bookRepo.AddBookAsync(newBook);


            foreach (var categoryId in model.SelectedCategoryIds)
            {

                var bookCategory = _mapper.Map<BookCategory>(model);
                bookCategory.CategoryId = categoryId;
                bookCategory.BookId = newBook.Id;
                
                await _bookCategoryRepo.AddAsync(bookCategory);
            }

            TempData["SuccessMessage"] = "Book added successfully!";

            return RedirectToAction(nameof(Index));


        }

        [HttpPost]
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
    }
}
