using Application.DTOs;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class Login : PageModel
{
    private readonly IAuthService _auth;

    public Login(IAuthService auth) => _auth = auth;

    [BindProperty]
    public LoginUserDto Dto { get; set; }

    public void OnGet()
    {
        // just render the form
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _auth.LoginAsync(Dto, HttpContext);
        if (result.IsFailed)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Message);
            return Page();
        }
        var (principal, props) = result.Value;

        // 1) Check profileCompleted claim
        var profileCompleted = principal.Claims
            .FirstOrDefault(c => c.Type == "profileCompleted")?.Value;

        if (!string.Equals(profileCompleted, "true", StringComparison.OrdinalIgnoreCase))
        {
            // Don't sign in yet—just store the tokens in a temp cookie or session
            // Then redirect them to your complete-profile UI
            // (You can still sign in if you like, so the UI can fetch the tokens)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props
            );
            return RedirectToPage("/CompleteProfile");
        }

        // 2) Normal full sign‑in and 200 OK
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        // redirect to home or protected area
        return RedirectToPage("/Index");
    }
}