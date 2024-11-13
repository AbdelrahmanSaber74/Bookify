namespace Bookify.Web.Repositories.RentalCopies
{
    public interface IRentalCopyRepo
    {
        Task<RentalCopy> GetByIdAsync(int id);
        Task<RentalCopy> GetByBookCopyIdAsync(int rentalId, int bookCopId);
        Task<int> CountRentalCopiesByIdAsync(int id);
        Task<IEnumerable<RentalCopy>> GetAllAsync();
        Task<IEnumerable<RentalCopy>> GetRentalHistoryAsync(int id);
        Task<IEnumerable<RentalCopy>> GetAllByRentalIdAsync(int rentalId);
        Task AddAsync(RentalCopy rentalCopy);
        Task UpdateAsync(RentalCopy rentalCopy);
        Task DeleteAsync(int id); Task<bool> AnyAsync(Expression<Func<RentalCopy, bool>> predicate);

    }
}
