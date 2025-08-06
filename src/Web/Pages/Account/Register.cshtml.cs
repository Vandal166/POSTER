using Application.DTOs;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;
using Web.Contracts;

namespace Web.Pages;

[RedirectAuthenticated] // allow only unauthenticated users to access this page
public class Register : PageModel
{
    private readonly IAuthService _auth;
    private readonly IToastBuilder _toastBuilder;

    public Register(IAuthService auth, IToastBuilder toastBuilder)
    {
        _auth = auth;
        _toastBuilder = toastBuilder;
    }

    [BindProperty]
    public RegisterUserDto Dto { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _auth.RegisterAsync(Dto, ct);
        if (result.IsFailed)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Message);
            return Page();
        }
        
        _toastBuilder.SetToast("Registration Successful, you can now log in", ToastType.Success)
            .Build(TempData);
            
        return RedirectToPage("/Account/Login");
    }
}