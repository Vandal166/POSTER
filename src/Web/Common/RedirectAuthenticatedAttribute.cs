using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Common;

public class RedirectAuthenticatedAttribute : Attribute, IPageFilter
{
    private string _redirectPage = "/Index";

    public string Page
    {
        get => _redirectPage;
        set => _redirectPage = value;
    }

    public RedirectAuthenticatedAttribute() { }

    public RedirectAuthenticatedAttribute(string redirectPage)
    {
        _redirectPage = redirectPage;
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            context.Result = new RedirectToPageResult(_redirectPage);
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
}