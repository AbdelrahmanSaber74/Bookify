
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]

    public class CategoriesController : Controller
    {
        private readonly ICategoriesRepo _categoriesRepo;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoriesRepo categoriesRepo, IMapper mapper)
        {
            _categoriesRepo = categoriesRepo;
            _mapper = mapper;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _categoriesRepo.GetAllCategoriesAsync();
            var categoryViewModels = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            return View(categoryViewModels);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View("AddCategory");
        }

        // GET: Categories/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoriesRepo.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            var model = _mapper.Map<CategoryViewModel>(category);
            return View("EditCategory", model);
        }

        // POST: Categories/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View("AddCategory", model);

            var newCategory = _mapper.Map<Category>(model);
            newCategory.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _categoriesRepo.AddCategoryAsync(newCategory);
            

            TempData["SuccessMessage"] = "Category added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Categories/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View("EditCategory", model);

            var existingCategory = await _categoriesRepo.GetCategoryByIdAsync(model.Id);
            if (existingCategory == null) return NotFound();

            _mapper.Map(model, existingCategory);
            existingCategory.LastUpdatedOn = DateTime.Now;
            existingCategory.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _categoriesRepo.UpdateCategoryAsync(existingCategory);

            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Categories/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existingCategory = await _categoriesRepo.GetCategoryByIdAsync(id);
            if (existingCategory == null) return Json(new { success = false, message = "Category not found." });

            await _categoriesRepo.DeleteCategoryAsync(id);
            return Json(new { success = true });
        }

        // POST: Categories/ToggleStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var existingCategory = await _categoriesRepo.GetCategoryByIdAsync(id);
            if (existingCategory == null) return NotFound();

            existingCategory.IsDeleted = !existingCategory.IsDeleted;
            existingCategory.LastUpdatedOn = DateTime.Now;
            existingCategory.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _categoriesRepo.UpdateCategoryAsync(existingCategory);

            return Json(new
            {
                success = true,
                lastUpdatedOn = existingCategory.LastUpdatedOn.ToString()
            });
        }

        // Check if category name is unique
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsCategoryNameUnique(string name, int id)
        {
            var isNameTaken = await _categoriesRepo.AnyAsync(c => c.Name == name && c.Id != id);
            if (isNameTaken) return Json(string.Format(Errors.Duplicated, name));

            return Json(true);
        }
    }
}
