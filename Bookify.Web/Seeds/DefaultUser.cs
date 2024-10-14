
namespace Bookify.Web.Seeds
{
    public static class DefaultUser
    {
        public static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {

            var adminEmail = "admin@bookify.com";
            var adminPassword = "Admin@123";

            ApplicationUser adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Admin User",
                PhoneNumber = "1234567890",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now,
            };

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {

                    await userManager.AddToRoleAsync(adminUser, UserEnum.Admin.ToString());
                }

            }



        }
    }
}
