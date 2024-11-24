using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Repositories.Reports
{
    public class ReportsRepo : IReportsRepo
    {
        private readonly ApplicationDbContext _context;

        public ReportsRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            throw new NotImplementedException();
        }



        public async Task<IEnumerable<RentalCopy>> GetRentalCopiesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.RentalCopies
                .Where(copy => copy.RentalDate >= startDate && copy.EndDate <= endDate)
                .ToListAsync();
        }

    }
}
