using Microsoft.AspNetCore.Mvc.Rendering;

namespace eventos_qr.Helpers
{
    public static class HtmlHelpers
    {
        public static string IsActive(this IHtmlHelper html,
                                      string controllers = null,
                                      string actions = null,
                                      string cssClass = "active")
        {
            var routeData = html.ViewContext.RouteData;

            var currentAction = routeData.Values["action"]?.ToString();
            var currentController = routeData.Values["controller"]?.ToString();

            var acceptedActions = (actions ?? currentAction)?.Split(',');
            var acceptedControllers = (controllers ?? currentController)?.Split(',');

            return (acceptedActions.Contains(currentAction, StringComparer.OrdinalIgnoreCase)
                    && acceptedControllers.Contains(currentController, StringComparer.OrdinalIgnoreCase))
                ? cssClass
                : string.Empty;
        }
    }
}
