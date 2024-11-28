
namespace Bookify.Web.Repositories.Repositories
{
	public class SubscribersRepo : ISubscribersRepo
	{
		private readonly IApplicationDbContext _context;

		public SubscribersRepo(IApplicationDbContext context)
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
					.Include(s => s.Rentals.Where(r => !r.IsDeleted))
						.ThenInclude(r => r.RentalCopies)
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

		public async Task<int> Count()
		{
			return await _context.Subscribers.CountAsync();
		}

		public async Task<List<ChartItemViewModel>> GetSubscribersPerCity()
		{
			return await _context.Subscribers
				.Include(s => s.Area)
				.GroupBy(s => s.Area!.Name)
				.Select(group => new ChartItemViewModel
				{
					Label = group.Key,
					Value = Convert.ToString(group.Count())
				})
				.ToListAsync();
		}

		
	}
}
