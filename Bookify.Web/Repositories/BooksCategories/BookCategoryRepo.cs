namespace Bookify.Web.Repositories.BooksCategories
{
    public class BookCategoryRepo : IBookCategoryRepo
    {

        private readonly ApplicationDbContext _context;

        public BookCategoryRepo(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(BookCategory bookCategory)
        {
            await _context.BookCategories.AddAsync(bookCategory); 
            await _context.SaveChangesAsync(); 
        }


        public async Task<IEnumerable<BookCategory>> GetAllAsync()
        {
            return await _context.BookCategories.ToListAsync(); // Get all categories
        }
    }
}
