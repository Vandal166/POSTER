using Application.Contracts;
using Application.DTOs;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class CompleteProfile : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _auth;

    public CompleteProfile(ICurrentUserService currentUser, IAuthService auth)
    {
        _currentUser = currentUser;
        _auth = auth;
    }

    [BindProperty]
    public CompleteProfileDto Dto { get; set; }
    
    
    public void OnGet()
    {
        
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _auth.CompleteProfileAsync(_currentUser.UserID.ToString(), Dto.Username);
        if (result.IsFailed)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Message);
            return Page();
        }
        
        //if successful, redirect to home or protected area
        
        return RedirectToPage("/Index");
    }
}