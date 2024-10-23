namespace Bookify.Web.Repositories.Governorates
{
    public class GovernorateRepo : IGovernorateRepo

    {

        private readonly ApplicationDbContext _context;

        public GovernorateRepo(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Governorate>> GetAllGovernoratesAsync()
        {
            return await _context.Governorates.ToListAsync();
        }
    }
}
