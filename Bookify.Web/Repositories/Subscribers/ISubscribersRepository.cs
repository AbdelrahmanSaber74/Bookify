
namespace Bookify.Web.Repositories.Repositories
{
    public interface ISubscribersRepository
    {
        Task<IEnumerable<Subscriber>> GetAllAsync();
        Task<Subscriber> GetByIdAsync(int id);
        Task AddAsync(Subscriber subscriber);   
        Task UpdateAsync(Subscriber subscriber);
        Task DeleteAsync(int id);
        Task SaveChangesAsync(); 

    }
}
