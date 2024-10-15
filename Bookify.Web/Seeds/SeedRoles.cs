using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Bookify.Web.Seeds
{
    public static class SeedRoles
    {
        public static async Task AddDefaultRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] {AppRoles.Admin, AppRoles.Archive ,AppRoles.Reception};

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
