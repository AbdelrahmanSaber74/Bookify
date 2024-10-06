namespace Bookify.Web.Repositories.BooksCategories
{
    public interface IBookCategoryRepo
    {
        Task AddAsync(BookCategory bookCategory);
        Task<IEnumerable<BookCategory>> GetAllAsync();
    }
}
