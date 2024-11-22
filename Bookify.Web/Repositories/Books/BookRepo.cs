using Bookify.Web.Core.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bookify.Web.Repositories.Books
{
    public class BookRepo : IBookRepo
    {
        private readonly ApplicationDbContext _context;

        public BookRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        #region CRUD Operations

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddBookAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            _context.Books.Update(book);
            await SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await SaveChangesAsync();
            }
        }

        public async Task<bool> AnyBookAsync(Expression<Func<Book, bool>> predicate)
        {
            return await _context.Books.AnyAsync(predicate);
        }

        #endregion

        #region Additional Queries

        public async Task<IEnumerable<Book>> GetAllBooksIncludeAuthorAsync()
        {
            return await _context.Books.Include(b => b.Author).ToListAsync();
        }

        public async Task<int> GetLatestBookIdAsync()
        {
            return await _context.Books
                .OrderByDescending(b => b.Id)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Book> GetBookByTitleAndAuthor(string title, int authorId)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.Title == title && b.AuthorId == authorId);
        }

        public async Task<IQueryable<Book>> GetAllBooksAsQueryableAsync()
        {
            return _context.Books.Include(b => b.Author);
        }

        public async Task<IEnumerable<Book>> TopBooks()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.CreatedOn)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> LastAddedBooks()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.CreatedOn)
                .Take(12)
                .ToListAsync();
        }

        public async Task<Book> GetBookByIdWithDetailsAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                    .ThenInclude(bc => bc.Category)
                .SingleOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<List<Book>> GetBooksWithDetailsAsync()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                    .ThenInclude(bc => bc.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookDTO>> FindBooks(string query)
        {
            return await _context.Books
                .Where(b => (b.Title.Contains(query) || b.Author!.Name.Contains(query)) && !b.IsDeleted)
                .Select(b => new BookDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author!.Name
                })
                .ToListAsync();
        }

        #endregion

        #region Helper Methods

        private async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }



        #endregion
    }
}
