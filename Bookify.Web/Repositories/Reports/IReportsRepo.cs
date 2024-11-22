namespace Bookify.Web.Repositories.Reports
{
    public interface IReportsRepo
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
    }
}
