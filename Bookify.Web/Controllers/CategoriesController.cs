using Bookify.Web.Repositories.Categories;

namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {

        private readonly ICategoriesRepo _categoriesRepo;

        public CategoriesController(ICategoriesRepo categoriesRepo)
        {
            _categoriesRepo = categoriesRepo;
        }
       //Todo : use view model
        public async Task<IActionResult> Index()
        {
            var categories =  await _categoriesRepo.GetAllCategoriesAsync();
            return View(categories);
        }


    }
}
