#pragma warning disable CS8019 // Unnecessary using directive (using Microsoft.AspNetCore.Mvc;)
// it is necessary for the code to work in not debug mode
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OgrenciAidatSistemi.Helpers.Controller
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DebugOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
#if DEBUG
            // Allow execution of the action method
            base.OnActionExecuting(context);
#else
        // Return a 404 Not Found result
        context.Result = new NotFoundResult();
#endif
        }
    }
}
