using Application.DTOs;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages;

[RedirectAuthenticated] // allow only unauthenticated users to access this page
public class Register : PageModel
{
    private readonly IAuthService _auth;

    public Register(IAuthService auth) => _auth = auth;

    [BindProperty]
    public RegisterUserDto Dto { get; set; }

    public void OnGet()
    {
        // just render the form
    }

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
        
        // redirect to home or protected area
        return RedirectToPage("/Account/Login");
    }
}