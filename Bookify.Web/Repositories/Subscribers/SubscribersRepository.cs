namespace Bookify.Web.Repositories.Repositories
{
    public class SubscribersRepository : ISubscribersRepository
    {
        private readonly ApplicationDbContext _context; 

        public SubscribersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subscriber>> GetAllAsync()
        {
            return await _context.Subscribers.ToListAsync();
        }

        public async Task<Subscriber> GetByIdAsync(int id)
        {
            return await _context.Subscribers.FindAsync(id);
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync(); 
        }
    }
}
