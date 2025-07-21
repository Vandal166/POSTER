using Application.DTOs;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var ct = new CancellationTokenSource();
        var result = await _auth.RegisterAsync(Dto, ct.Token);
        if (result.IsFailed)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Message);
            return Page();
        }

        // redirect to home or protected area
        return RedirectToPage("/Login");
    }
}