using System.Linq.Expressions;

namespace Bookify.Web.Repositories.Categories
{
    public interface ICategoriesRepo
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
        Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate);

    }
}
