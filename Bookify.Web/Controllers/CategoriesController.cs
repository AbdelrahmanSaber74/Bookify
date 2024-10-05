using Bookify.Web.Repositories.Categories;
using AutoMapper;

namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoriesRepo _categoriesRepo;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoriesRepo categoriesRepo, IMapper mapper)
        {
            _categoriesRepo = categoriesRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoriesRepo.GetAllCategoriesAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
            return View(categoryDtos);
        }

        public IActionResult Create()
        {
            return View("AddCategory");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoriesRepo.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            var categoryDto = _mapper.Map<CategoryDTO>(category);
            return View("EditCategory", categoryDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryDTO categoryDto)
        {
            if (!ModelState.IsValid) return View(categoryDto);

            var newCategory = _mapper.Map<Category>(categoryDto);
            await _categoriesRepo.AddCategoryAsync(newCategory);

            TempData["SuccessMessage"] = "Category added successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(CategoryDTO categoryDto)
        {
            if (!ModelState.IsValid) return View(categoryDto);

            var existingCategory = await _categoriesRepo.GetCategoryByIdAsync(categoryDto.Id);
            if (existingCategory == null) return NotFound();

            var updatedCategory = _mapper.Map(categoryDto, existingCategory);
            updatedCategory.LastUpdatedOn = DateTime.Now;

            await _categoriesRepo.UpdateCategoryAsync(updatedCategory);

            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existingCategory = await _categoriesRepo.GetCategoryByIdAsync(id);
            if (existingCategory == null) return Json(new { success = false, message = "Category not found." });

            await _categoriesRepo.DeleteCategoryAsync(id);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var existingCategory = await _categoriesRepo.GetCategoryByIdAsync(id);
            if (existingCategory == null) return NotFound();

            existingCategory.IsDeleted = !existingCategory.IsDeleted;
            existingCategory.LastUpdatedOn = DateTime.Now;

            await _categoriesRepo.UpdateCategoryAsync(existingCategory);

            return Json(new
            {
                success = true,
                lastUpdatedOn = existingCategory.LastUpdatedOn.ToString()
            });
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsCategoryNameUnique(string name, int id)
        {
            var isNameTaken = await _categoriesRepo.AnyAsync(c => c.Name == name && c.Id != id);
            if (isNameTaken) return Json($"The category name '{name}' is already taken.");

            return Json(true);
        }
    }
}
