namespace Bookify.Web.Repositories.Rentals
{
    public interface IRentalRepo
    {
        Task<bool> AnyAsync(Expression<Func<Rental, bool>> predicate);
        Task<Rental> GetByIdAsync(int id);
        Task<Rental> GetRentalWithCopiesByIdAsync(int id);
        Task<IEnumerable<Rental>> GetAllAsync();
        Task<IEnumerable<Rental>> GetAllBySubscriberIdAsync(int subscriberId);
        Task AddAsync(Rental rental);
        Task UpdateAsync(Rental rental);
        Task DeleteAsync(int id);
        Task<List<RentalCopy>> GetDelayedRentalsAsync();

    }
}
