namespace Bookify.Web.Repositories.BookCopies
{
    public class BookCopyRepo : IBookCopyRepo
    {
        private readonly ApplicationDbContext _context;

        public BookCopyRepo(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<BookCopy>> GetBookCopiesByBookIdAsync(int bookId)
        {
            return await _context.BookCopies
                .Where(m => m.BookId == bookId) // Filter by bookId
                .Select(m => new BookCopy // Select specific properties
                {
                    Id = m.Id,
                    BookId = m.BookId,
                    EditorNumber = m.EditorNumber,
                    IsAvailableForRental = m.IsAvailableForRental,
                    SerialNumber = m.SerialNumber,
                    IsDeleted = m.IsDeleted,
                    CreatedOn = m.CreatedOn,
                    LastUpdatedOn = m.LastUpdatedOn,
                })
                .ToListAsync(); // Convert the result to a list asynchronously
        }

        public async Task<BookCopy> GetBookCopyByIdAsync(int id)
        {
            return await _context.BookCopies.FindAsync(id);
        }


        public async Task<BookCopy> UpdateBookCopyAsync(BookCopy bookCopy)
        {
            _context.BookCopies.Update(bookCopy);
            await _context.SaveChangesAsync();
            return bookCopy;
        }
    }
}
