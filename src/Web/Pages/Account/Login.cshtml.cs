using Application.DTOs;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages;

[RedirectAuthenticated] // only unauthenticated users to access this page
public class Login : PageModel
{
    private readonly IAuthService _auth;

    public Login(IAuthService auth) => _auth = auth;

    [BindProperty]
    public LoginUserDto Dto { get; set; }
    
    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _auth.LoginAsync(Dto, ct);
        if (result.IsFailed)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Message);
            return Page();
        }
        var (principal, props) = result.Value;
        
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        return RedirectToPage("/Index");
    }
}