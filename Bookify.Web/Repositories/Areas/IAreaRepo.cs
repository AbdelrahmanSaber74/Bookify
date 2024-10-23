namespace Bookify.Web.Repositories.Areas
{
    public interface IAreaRepo
    {
        Task<IEnumerable<Area>> GetAllAreasAsync(); 

        Task<IEnumerable<Area>> GetAreasByGovernorateIdAsync(int governorateId); 

    }
}
