using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Aplikacija.Models
{
    public class EmployeeAuthAttribute : ActionFilterAttribute
    {
        private readonly AlgebraDatabaseEntities context = new AlgebraDatabaseEntities();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies["authCookie"];

            if (cookie != null)
            {
                string username = cookie["username"];
                string token = cookie["token"];

                if(!IsEmployeeValid(username, token))
                {
                    filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "Controller", "Auth" }, { "Action", "Login" } });
                }
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "Controller", "Auth" }, { "Action", "Login" } });
            }

            base.OnActionExecuting(filterContext);
        }

        private bool IsEmployeeValid(string username, string token)
        {
            Employees employeeDb = context.Employees.Where(c => c.Username == username).SingleOrDefault();

            context.Entry(employeeDb).Reload();

            if (employeeDb == null || token != employeeDb.Token) return false;

            return true;
        }
    }
}