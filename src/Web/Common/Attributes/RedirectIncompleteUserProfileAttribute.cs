using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Common;

/// <summary>
/// Redirects authenticated users to the profile completion page if their profile is incomplete.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RedirectIncompleteUserProfileAttribute : Attribute, IPageFilter
{
    public RedirectIncompleteUserProfileAttribute() {}
    
    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated == true && 
            user.HasClaim("profileCompleted", "false"))
        {
            context.HttpContext.Response.Headers["HX-Redirect"] = "/Account/CompleteProfile";
            context.Result = new RedirectToPageResult("/Account/CompleteProfile")
            {
                Permanent = true
            };
        }
    }
    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
}