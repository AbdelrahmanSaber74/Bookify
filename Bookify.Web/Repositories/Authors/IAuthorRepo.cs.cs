using System.Linq.Expressions;

namespace Bookify.Web.Repositories.Authors
{
    public interface IAuthorRepo
    {
        Task<IEnumerable<Author>> GetAllAuthorsAsync();
        Task<Author> GetAuthorByIdAsync(int id);
        Task AddAuthorAsync(Author author);
        Task UpdateAuthorAsync(Author author);
        Task DeleteAuthorAsync(int id);
        Task<bool> AnyAsync(Expression<Func<Author, bool>> predicate);
    }
}
