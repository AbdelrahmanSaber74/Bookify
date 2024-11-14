
namespace Bookify.Web.Helpers
{
    public static class MenuHelper
    {
        public static bool IsActive(ViewContext viewContext, string controller, string action)
        {
            if (viewContext == null || viewContext.RouteData == null)
            {
                return false;
            }

            var currentController = viewContext.RouteData.Values["controller"]?.ToString();
            var currentAction = viewContext.RouteData.Values["action"]?.ToString();

            return string.Equals(currentController, controller, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(currentAction, action, StringComparison.OrdinalIgnoreCase);
        }
    }
}
