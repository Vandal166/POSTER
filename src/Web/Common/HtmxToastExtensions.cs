using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Web.Common;

public static class HtmxToastExtensions
{
    /// <summary>
    /// Appends an htmx header (HX-Trigger) to the response so that it triggers a toast notification and returns the original Partial view.
    /// </summary>
    public static IActionResult WithHxToast(this PartialViewResult result, HttpContext httpContext, string message, string type = "success")
    {
        httpContext?.Response.Headers.Append("HX-Trigger", JsonSerializer.Serialize(new
        {
            showToast = new { message, type }
        }));
        return result;
    }

    public static RedirectToPageResult WithHxToast(this RedirectToPageResult result, HttpContext httpContext, string message, string type = "success")
    {
        httpContext?.Response.Headers.Append("HX-Trigger", JsonSerializer.Serialize(new
        {
            showToast = new { message, type }
        }));
        return result;
    }
}