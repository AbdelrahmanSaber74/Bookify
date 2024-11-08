namespace Bookify.Web.Repositories.Rentals
{
    public interface IRentalRepo
    {

        Task<Rental> GetCopyDetails(string id);
    }
}
