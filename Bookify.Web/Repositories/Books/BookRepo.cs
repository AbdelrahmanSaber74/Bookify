using Bookify.Web.Core.DTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bookify.Web.Repositories.Books
{
    public class BookRepo : IBookRepo
    {
        private readonly ApplicationDbContext _context;

        public BookRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _context.Books.ToListAsync(); // Get all books from the database
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _context.Books
                   .Include(b => b.Author)
                   .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddBookAsync(Book book)
        {
            await _context.Books.AddAsync(book); // Add a new book to the database
            await SaveChangesAsync(); // Save changes to the database
        }

        public async Task UpdateBookAsync(Book book)
        {
            _context.Books.Update(book); // Update the existing book
            await SaveChangesAsync(); // Save changes to the database
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id); // Find the book to delete
            if (book != null)
            {
                _context.Books.Remove(book); // Remove the book from the database
                await SaveChangesAsync(); // Save changes to the database
            }
        }

        public async Task<bool> AnyBookAsync(Expression<Func<Book, bool>> predicate)
        {
            return await _context.Books.AnyAsync(predicate); // Check if any book matches the predicate
        }


        public async Task<IEnumerable<Book>> GetAllBooksIncludeAuthorAsync()
        {
            return await _context.Books.Include(b => b.Author).ToListAsync();
        }


        public async Task<int> GetLatestBookIdAsync()
        {
            int lastBook = await _context.Books
                .OrderByDescending(b => b.Id)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();

            return lastBook;
        }



        // Private method to save changes to the database
        private async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync(); // Save changes to the database
        }


        public async Task<Book> GetBookByTitleAndAuthor(string title, int authorId)
        {
            return await _context.Books
                .Where(b => b.Title == title && b.AuthorId == authorId)
                .FirstOrDefaultAsync();
        }

        public async Task<IQueryable<Book>> GetAllBooksAsQueryableAsync()
        {
            return _context.Books.Include(b => b.Author); // Returning IQueryable directly
        }

        public async Task<IEnumerable<Book>> TopBooks()
        {
            return await _context.Books
                .Include(a => a.Author)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(b => b.CreatedOn)
                .Take(10)
                .ToListAsync();
        }


        public async Task<IEnumerable<Book>> LastAddedBooks()
        {
            return await _context.Books
                            .Include(a => a.Author)
                            .Where(r => !r.IsDeleted)
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
                              .ThenInclude(b => b.Category)
                              .SingleOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<IEnumerable<BookDTO>> FindBooks(string query)
        {
            // Retrieve the books matching the query criteria and map them to BookDTO
            var books = await _context.Books
                .Where(b => (b.Title.Contains(query) || b.Author!.Name.Contains(query)) && !b.IsDeleted)
                .Select(b => new BookDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author!.Name
                })
                .ToListAsync();

            return books;
        }





    }
}
