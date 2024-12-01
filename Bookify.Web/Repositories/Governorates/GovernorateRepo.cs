namespace Bookify.Web.Repositories.Governorates
{
    public class GovernorateRepo : IGovernorateRepo

    {

        private readonly IApplicationDbContext _context;

        public GovernorateRepo(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Governorate>> GetAllGovernoratesAsync()
        {
            return await _context.Governorates.Where(m => !m.IsDeleted).ToListAsync();
        }
    }
}
