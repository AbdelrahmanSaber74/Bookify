namespace Bookify.Web.Repositories.Governorates
{
    public interface IGovernorateRepo
    {
        public Task<IEnumerable<Governorate>> GetAllGovernoratesAsync();

    }
}
