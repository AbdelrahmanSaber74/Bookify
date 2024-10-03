using Bookify.Web.Core.Models;
using Bookify.Web.DTO;
using Bookify.Web.Repositories.Categories;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoriesRepo _categoriesRepo;

        public CategoriesController(ICategoriesRepo categoriesRepo)
        {
            _categoriesRepo = categoriesRepo;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoriesRepo.GetAllCategoriesAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View("AddCategory");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoriesRepo.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = MapToDto(category);
            return View("EditCategory", categoryDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryDTO categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryDto);
            }

            var newCategory = new Category
            {
                Name = categoryDto.Name,
                CreatedOn = DateTime.Now,
                IsDeleted = categoryDto.IsDeleted,
            };

            await _categoriesRepo.AddCategoryAsync(newCategory);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(CategoryDTO categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryDto);
            }

            var existingCategory = await _categoriesRepo.GetCategoryByIdAsync(categoryDto.Id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = categoryDto.Name;
            existingCategory.IsDeleted = categoryDto.IsDeleted;
            existingCategory.LastUpdatedOn = DateTime.Now;

            await _categoriesRepo.UpdateCategoryAsync(existingCategory);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var existingCategory = await _categoriesRepo.GetCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            await _categoriesRepo.DeleteCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private CategoryDTO MapToDto(Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                IsDeleted = category.IsDeleted,
            };
        }
    }
}
