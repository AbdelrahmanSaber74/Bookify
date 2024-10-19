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


		public async Task<List<int>> GetCategoryIdsByBookIdAsync(int bookId)
		{
			return await _context.BookCategories
				.Where(bc => bc.BookId == bookId)
				.Select(bc => bc.CategoryId)
				.ToListAsync();
		}




		public async Task<BookCategory> GetBookCategoryByIdsAsync(int bookId, int categoryId)
		{
			return await _context.BookCategories
				.FirstOrDefaultAsync(bc => bc.BookId == bookId && bc.CategoryId == categoryId);
		}

		public async Task RemoveAsync(BookCategory bookCategory)
		{
			if (bookCategory != null)
			{
				_context.BookCategories.Remove(bookCategory);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> RemoveBookCategoryByBookIdAsync(int bookId)
		{
			var bookCategory = await _context.BookCategories.FindAsync(bookId);

			if (bookCategory != null)
			{
				_context.BookCategories.Remove(bookCategory);
				await _context.SaveChangesAsync();
				return true;
			}

			return false;
		}

	}
}
