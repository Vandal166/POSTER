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
    private readonly ILogger<ErrorModel> _logger;
    private readonly IToastBuilder _toastBuilder;
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);


    public ErrorModel(ILogger<ErrorModel> logger, IToastBuilder toastBuilder)
    {
        _logger = logger;
        _toastBuilder = toastBuilder;
    }

    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _toastBuilder.SetToast("An error occurred while processing your request.", ToastType.Error).Build(TempData);
    }
}