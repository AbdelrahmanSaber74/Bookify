using System.Linq.Expressions;

namespace Bookify.Web.Repositories.Categories
{
    public class CategoriesRepo : ICategoriesRepo
    {
        private readonly ApplicationDbContext _context;

        public CategoriesRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await FindCategoryByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await SaveChangesAsync();
            }
            
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await FindCategoryByIdAsync(id);
            return category;
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await SaveChangesAsync();
        }

        // Implementation of AnyAsync method
        public async Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate)
        {
            return await _context.Categories.AnyAsync(predicate);
        }

        // Helper methods
        private async Task<Category> FindCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        private async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
