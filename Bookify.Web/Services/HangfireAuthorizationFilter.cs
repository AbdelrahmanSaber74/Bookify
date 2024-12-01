using Hangfire.Dashboard;

namespace Bookify.Web.Services
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return httpContext.User.IsInRole(AppRoles.Admin);
        }
    }

}
