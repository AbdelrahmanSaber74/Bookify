using Bookify.Domain.DTO;

namespace Bookify.Web.Repositories.Books
{
    public interface IBookRepo
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<IEnumerable<Book>> GetAllBooksIncludeAuthorAsync();
        Task<IEnumerable<BookDTO>> FindBooks(string query);
        Task<IEnumerable<Book>> TopBooks();
        Task<IEnumerable<Book>> LastAddedBooks();
        Task<Book> GetBookByIdWithDetailsAsync(int id);
        Task<List<Book>> GetBooksWithDetailsAsync();
        Task<Book> GetBookByIdAsync(int id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task<int> GetLatestBookIdAsync();
        Task DeleteBookAsync(int id);
        Task<bool> AnyBookAsync(Expression<Func<Book, bool>> predicate);

        Task<Book> GetBookByTitleAndAuthor(string title, int authorId);
        Task<IQueryable<Book>> GetAllBooksAsQueryableAsync(); // Asynchronous method to get IQueryable<Book>

    }
}
