using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Aplikacija.Models
{
    public class NoCookieAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies["authCookie"];

            if (cookie != null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "Controller", "Home" }, { "Action", "Index" } });
            }

            base.OnActionExecuting(filterContext);
        }
    }
}