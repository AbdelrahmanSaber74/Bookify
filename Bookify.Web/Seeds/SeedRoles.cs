using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Bookify.Web.Seeds
{
    public static class SeedRoles
    {
        public static async Task AddDefaultRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { UserEnum.Admin.ToString(), UserEnum.Reception.ToString() , UserEnum.Archive.ToString() };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
