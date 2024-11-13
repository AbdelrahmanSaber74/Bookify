namespace Bookify.Web.Repositories.Rentals
{
    public class RentalRepo : IRentalRepo
    {
        private readonly ApplicationDbContext _context;

        public RentalRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rental> GetByIdAsync(int id)
        {
            return await _context.Rentals.FindAsync(id);
        }

        public async Task<Rental> GetRentalWithCopiesByIdAsync(int id)
        {
            return await _context.Rentals
                                 .Include(r => r.RentalCopies)
                                 .ThenInclude(r => r.BookCopy)
                                 .ThenInclude(r => r!.Book)
                                 .SingleOrDefaultAsync(r => r.Id == id);
        }


        public async Task<IEnumerable<Rental>> GetAllAsync()
        {
            return await _context.Rentals.ToListAsync();
        }

        public async Task AddAsync(Rental rental)
        {
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Rental rental)
        {
            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> AnyAsync(Expression<Func<Rental, bool>> predicate)
        {
            return await _context.Rentals.AnyAsync(predicate);
        }

        public async Task<IEnumerable<Rental>> GetAllBySubscriberIdAsync(int subscriberId)
        {
            return await _context.Rentals
                                 .Where(r => r.SubscriberId == subscriberId && !r.IsDeleted)
                                 .Include(r => r.RentalCopies)
                                 .ToListAsync();
        }


      
    }
}
