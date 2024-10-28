using Bookify.Web.Core.Models;

namespace Bookify.Web.Repositories.Repositories
{
    public class SubscribersRepo : ISubscribersRepo
    {
        private readonly ApplicationDbContext _context;

        public SubscribersRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subscriber>> GetAllAsync()
        {
            return await _context.Subscribers.ToListAsync();
        }

        public async Task<Subscriber> GetByIdAsync(int id)
        {
            return await _context.Subscribers
                .Include(s => s.Governorate)
                .Include(s => s.Area)
                .Include(s => s.subscriptions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Subscriber subscriber)
        {
            await _context.Subscribers.AddAsync(subscriber);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(Subscriber subscriber)
        {
            _context.Subscribers.Update(subscriber);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var subscriber = await GetByIdAsync(id);
            if (subscriber != null)
            {
                _context.Subscribers.Remove(subscriber);
                await SaveChangesAsync();
            }
        }

        public async Task<Subscriber> FindSubscriberAsync(Expression<Func<Subscriber, bool>> predicate)
        {
            return await _context.Subscribers.FirstOrDefaultAsync(predicate);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Subscriber> Search(string value)
        {
            return await _context.Subscribers
                .SingleOrDefaultAsync(m => m.NationalId == value || m.MobileNumber == value || m.Email == value);
        }


    }
}
