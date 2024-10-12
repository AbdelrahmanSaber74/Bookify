namespace Bookify.Web.Helpers
{
    public static class MenuHelper
    {
        public static bool IsActive(ViewContext viewContext, string controller, string action)
        {
            var currentController = viewContext.RouteData.Values["controller"].ToString();
            var currentAction = viewContext.RouteData.Values["action"].ToString();
            return currentController.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                   currentAction.Equals(action, StringComparison.OrdinalIgnoreCase);
        }
    }
}
