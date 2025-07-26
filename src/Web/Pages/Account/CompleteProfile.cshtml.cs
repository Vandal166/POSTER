using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

[Authorize(Policy = "ProfileNotCompleted")] // only allow authenticated users who haven't completed their profile
public class CompleteProfile : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserService _userService;

    public CompleteProfile(ICurrentUserService currentUser, IUserService userService)
    {
        _currentUser = currentUser;
        _userService = userService;
    }

    [BindProperty]
    public UsernameDto Dto { get; set; }
    
    
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _userService.UpdateUsernameAsync(_currentUser.ID, Dto, ct);
        if (result.IsFailed)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Message);
            return Page();
        }
        
        return RedirectToPage("/Index");
    }
}