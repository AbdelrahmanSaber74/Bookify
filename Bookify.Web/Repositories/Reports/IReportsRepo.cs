namespace Bookify.Web.Repositories.Reports
{
    public interface IReportsRepo
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<IEnumerable<RentalCopy>> GetRentalCopiesAsync(DateTime start , DateTime end);
    }
}
