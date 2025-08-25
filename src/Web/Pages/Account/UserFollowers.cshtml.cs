using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class UserFollowers : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepo;
    private readonly IFollowService _followService;
    [BindProperty]
    public UserProfileLoaderViewModel UserProfileLoader { get; set; } = new UserProfileLoaderViewModel();
    
    public UserFollowers(ICurrentUserService currentUser, IUserRepository userRepo, IFollowService followService)
    {
        _currentUser = currentUser;
        _userRepo = userRepo;
        _followService = followService;
    }
    
    
    // id is userID
    public async Task<IActionResult> OnGetAsync(string identifier, CancellationToken ct = default)
    {
        UserProfileDto? user;
        if (Guid.TryParse(identifier, out var userId))
        {
            user = await _userRepo.GetUserProfileDtoAsync(userId, ct);
        }
        else
        {
            user = await _userRepo.GetUserProfileDtoByNameAsync(identifier, ct);
        }
        if(user is null || user.Id == Guid.Empty)
            return NotFound();

        UserProfileLoader = new UserProfileLoaderViewModel
        {
            User = user,
            Followers = await _followService.GetFollowersAsync(user.Id, ct),
            Following = await _followService.GetFollowingAsync(user.Id, ct),
            IsFollowing = await _followService.IsFollowingAsync(_currentUser.ID, user.Id, ct),
            CurrentUserFollowingIDs = await _followService.GetFollowingIDsAsync(_currentUser.ID, ct)
        };
        return Page();
    }
}