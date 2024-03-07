using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
