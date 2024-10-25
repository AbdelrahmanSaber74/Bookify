
namespace Bookify.Web.Repositories.Repositories
{
    public interface ISubscribersRepo
    {
        Task<IEnumerable<Subscriber>> GetAllAsync();
        Task<Subscriber> GetByIdAsync(int id);
        Task<Subscriber> FindSubscriberAsync(Expression<Func<Subscriber, bool>> predicate);
        Task AddAsync(Subscriber subscriber);   
        Task UpdateAsync(Subscriber subscriber);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
		Task<Subscriber> Search(string value);


	}
}
