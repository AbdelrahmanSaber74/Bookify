namespace Bookify.Web.Repositories.RentalCopies
{
    public class RentalCopyRepo : IRentalCopyRepo
    {
        private readonly ApplicationDbContext _context;

        public RentalCopyRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RentalCopy> GetByIdAsync(int id)
        {
            return await _context.RentalCopies.FindAsync(id);
        }

        public async Task<IEnumerable<RentalCopy>> GetAllAsync()
        {
            return await _context.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .ToListAsync();

        }

        public async Task AddAsync(RentalCopy rentalCopy)
        {
            await _context.RentalCopies.AddAsync(rentalCopy);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RentalCopy rentalCopy)
        {
            _context.RentalCopies.Update(rentalCopy);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rentalCopy = await _context.RentalCopies.FindAsync(id);
            if (rentalCopy != null)
            {
                _context.RentalCopies.Remove(rentalCopy);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> AnyAsync(Expression<Func<RentalCopy, bool>> predicate)
        {
            return await _context.RentalCopies.AnyAsync(predicate);
        }

        public async Task<int> CountRentalCopiesByIdAsync(int id)
        {
            return await _context.RentalCopies.CountAsync(r => r.RentalId == id);
        }

        public async Task<IEnumerable<RentalCopy>> GetRentalHistoryAsync(int id)
        {
            return await _context.RentalCopies
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriber)
                .Where(r => r.BookCopyId == id)
                .OrderByDescending(r => r.RentalDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<RentalCopy>> GetAllByRentalIdAsync(int rentalId)
        {
            return await _context.RentalCopies
                .Where(r => r.RentalId == rentalId)
                .ToListAsync();
        }

        public async Task<RentalCopy> GetByBookCopyIdAsync(int rentalId, int bookCopId)
        {
            return await _context.RentalCopies
                            .Where(r => r.RentalId == rentalId && r.BookCopyId == bookCopId)
                            .FirstOrDefaultAsync();
        }

        public async Task<List<ChartItemViewModel>> GetRentalsPerDayAsync(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-29);
            endDate ??= DateTime.Today;

            var data = await _context.RentalCopies
                .Where(c => c.RentalDate >= startDate && c.RentalDate <= endDate)
                .GroupBy(c => new { Date = c.RentalDate.Date })
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key.Date.ToString("d MMM"),
                    Value = g.Count().ToString()
                })
                .ToListAsync();

            return data;
        }

        public async Task<IEnumerable<RentalCopy>> GetRentalsCopyForReport(DateTime startDate, DateTime endDate)
        {
            var result = await _context.RentalCopies
                .Where(copy => copy.RentalDate >= startDate && copy.EndDate <= endDate)
                .ToListAsync();

            return result;
        }


    }
}
