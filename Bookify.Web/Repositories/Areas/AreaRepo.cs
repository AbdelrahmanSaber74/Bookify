﻿using Bookify.Application.Common.Interfaces;
namespace Bookify.Web.Repositories.Areas
{
    public class AreaRepo : IAreaRepo
    {
        private readonly IApplicationDbContext _context;

        public AreaRepo(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Area>> GetAllAreasAsync()
        {
            return await _context.Areas.Where(m => !m.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<Area>> GetAreasByGovernorateIdAsync(int governorateId)
        {
            var areas = await _context.Areas
         .Where(a => a.GovernorateId == governorateId && !a.IsDeleted)
         .OrderBy(b => b.Name)
         .ToListAsync();

            if (areas == null || !areas.Any())
            {
                return Enumerable.Empty<Area>();
            }

            return areas;
        }
    }
}
