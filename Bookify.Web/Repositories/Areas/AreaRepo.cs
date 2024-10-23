namespace Bookify.Web.Repositories.Areas
{
    public class AreaRepo : IAreaRepo
    {
        private readonly ApplicationDbContext _context;

        public AreaRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Area>> GetAllAreasAsync()
        {
            return await _context.Areas.ToListAsync();
        }

        public async Task<IEnumerable<Area>> GetAreasByGovernorateIdAsync(int governorateId)
        {
            var areas = await _context.Areas
         .Where(a => a.GovernorateId == governorateId && !a.IsDeleted)
         .OrderBy(b => b.Name)
         .ToListAsync();

            if (areas == null || !areas.Any())
            {
                // يمكنك إضافة رسالة أو إجراء مختلف هنا إذا لم تكن هناك مناطق متاحة
                return Enumerable.Empty<Area>(); // إرجاع قائمة فارغة بدلاً من null
            }

            return areas;
        }
    }
}
