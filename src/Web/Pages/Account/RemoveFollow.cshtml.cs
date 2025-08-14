using Application.Contracts;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

[Authorize]
public class RemoveFollow : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IFollowService _followService;
    
    public RemoveFollow(ICurrentUserService currentUser, IFollowService followService)
    {
        _currentUser = currentUser;
        _followService = followService;
    }
    public IActionResult OnGet() => RedirectToPage("/Index");
    
    public async Task<IActionResult> OnPostAsync(Guid userId, CancellationToken ct = default)
    { 
        await _followService.UnfollowUserAsync(_currentUser.ID, userId, ct);
        return RedirectToPage("/Account/Profile", new { id = userId });
    }
}