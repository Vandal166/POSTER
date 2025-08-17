using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Common;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RedirectAuthenticatedAttribute : Attribute, IPageFilter
{
    public string Page { get; set; } = "/Index";

    public RedirectAuthenticatedAttribute() { }

    public RedirectAuthenticatedAttribute(string redirectPage)
    {
        Page = redirectPage;
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            context.Result = new RedirectToPageResult(Page);
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
}