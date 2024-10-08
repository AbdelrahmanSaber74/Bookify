
namespace Bookify.Web.Repositories.Books
{
    public interface IBookRepo
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<IEnumerable<Book>> GetAllBooksIncludeAuthorAsync();
        Task<Book> GetBookByIdAsync(int id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task<int> GetLatestBookIdAsync();
        Task DeleteBookAsync(int id);
        Task<bool> AnyBookAsync(Expression<Func<Book, bool>> predicate);

        Task<Book> GetBookByTitleAndAuthor (string title, int authorId);

    }
}
