using System.Diagnostics;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly IToastBuilder _toastBuilder;
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    
    public ErrorModel(IToastBuilder toastBuilder)
    {
        _toastBuilder = toastBuilder;
    }

    public int ErrorStatusCode { get; set; } = 500;
    public string? ErrorMessage { get; set; }

    public void OnGet(int? code = null)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        if (code.HasValue)
        {
            ErrorStatusCode = code.Value;
        }
        ErrorMessage = code switch
        {
            400 => "Bad request. The server could not understand your request.",
            401 => "Unauthorized. Authentication is required.",
            403 => "Forbidden. You do not have permission to access this resource.",
            404 => "The page you are looking for was not found.",
            405 => "Method not allowed. The request method is not supported.",
            408 => "Request timeout. The server timed out waiting for your request.",
            429 => "Too many requests. Please try again later.",
            500 => "An internal server error occurred.",
            502 => "Bad gateway. Invalid response from upstream server.",
            503 => "Service unavailable. The server is not ready to handle the request.",
            504 => "Gateway timeout. No timely response from upstream server.",
            _ => "An error occurred while processing your request."
        };

        _toastBuilder.SetToast(ErrorMessage, ToastType.Error).Build(TempData);
    }
}