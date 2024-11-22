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
    }
}
