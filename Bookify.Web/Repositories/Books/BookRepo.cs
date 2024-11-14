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
            return await _context.Books.FindAsync(id); // Find a book by ID
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
				.OrderByDescending(b => b.CreatedOn)
				.Take(10)
				.ToListAsync();
		}


		public async Task<IEnumerable<Book>> LastAddedBooks()
		{
			return await _context.Books
							.OrderBy(b => b.CreatedOn)
							.Take(12)
							.ToListAsync();
		}
	}
}
