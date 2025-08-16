using Application.Contracts;
using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

[Authorize]
public class AddFollow : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IFollowService _followService;
    private readonly INotificationRepository _notificationRepo;
    private readonly IFollowNotifier _followNotifier;
    
    public AddFollow(ICurrentUserService currentUser, IFollowService followService, INotificationRepository notificationRepo, IFollowNotifier followNotifier)
    {
        _currentUser = currentUser;
        _followService = followService;
        _notificationRepo = notificationRepo;
        _followNotifier = followNotifier;
    }
    public IActionResult OnGet() => RedirectToPage("/Index");
    
    public async Task<IActionResult> OnPostAsync(Guid userId, CancellationToken ct = default)
    { 
       var result = await _followService.FollowUserAsync(_currentUser.ID, userId, ct);
       if (!result)
           return RedirectToPage("/Account/Profile", new { identifier = userId });
       
       await _followNotifier.NotifyFollowedAsync(_currentUser.ID, userId, ct);
       await _notificationRepo.AddAndSaveAsync(Notification.Create(userId, $"{_currentUser.Username} has followed you.",
           $"/Account/Profile/{_currentUser.Username}").Value, ct);

       return RedirectToPage("/Account/Profile", new { identifier = userId });
    }
}