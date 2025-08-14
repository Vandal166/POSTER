using Application.Contracts;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

[Authorize]
public class AddFollow : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IFollowService _followService;
    
    public AddFollow(ICurrentUserService currentUser, IFollowService followService)
    {
        _currentUser = currentUser;
        _followService = followService;
    }
    public IActionResult OnGet() => RedirectToPage("/Index");
    
    public async Task<IActionResult> OnPostAsync(Guid userId, CancellationToken ct = default)
    { 
        await _followService.FollowUserAsync(_currentUser.ID, userId, ct);
        return RedirectToPage("/Account/Profile", new { id = userId });
    }
}