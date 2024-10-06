using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Repositories.Authors
{
    public class AuthorRepo : IAuthorRepo
    {
        private readonly ApplicationDbContext _context;

        public AuthorRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAuthorAsync(Author author)
        {
            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<Author, bool>> predicate)
        {
            return await _context.Authors.AnyAsync(predicate);
        }

        public async Task DeleteAuthorAsync(int id)
        {
            var author = await FindAuthorByIdAsync(id);
            if (author != null)
            {
                _context.Authors.Remove(author);
                await SaveChangesAsync();
            }
            else
            {
                // Optionally throw an exception or log the error
                throw new KeyNotFoundException($"Author with id {id} not found.");
            }
        }

        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            return await _context.Authors.ToListAsync();
        }

        public async Task<Author> GetAuthorByIdAsync(int id)
        {
            return await FindAuthorByIdAsync(id);
        }

        public async Task<IEnumerable<Author>> GetAvailableAuthorsAsync()
        {
            return await _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToListAsync();
        }

        public async Task UpdateAuthorAsync(Author author)
        {
            _context.Authors.Update(author);
            await SaveChangesAsync();
        }

        // Helper methods
        private async Task<Author> FindAuthorByIdAsync(int id)
        {
            return await _context.Authors.FindAsync(id);
        }

        private async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
