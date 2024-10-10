namespace Bookify.Web.Repositories.BooksCategories
{
    public interface IBookCategoryRepo
    {
        Task AddAsync(BookCategory bookCategory);
        Task<IEnumerable<BookCategory>> GetAllAsync();
        Task<List<int>> GetCategoryIdsByBookIdAsync(int bookId);
        Task RemoveAsync(BookCategory bookCategory);
        Task <bool> RemoveBookCategoryByBookIdAsync(int BookId);
        Task<BookCategory> GetBookCategoryByIdsAsync(int bookId, int categoryId);


    }
}
