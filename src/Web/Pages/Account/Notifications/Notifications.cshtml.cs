using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

[Authorize]
public class Notifications : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationRepository _notificationRepo;
    
    public Notifications(ICurrentUserService currentUser, INotificationRepository notificationRepo)
    {
        _currentUser = currentUser;
        _notificationRepo = notificationRepo;
    }

    [BindProperty]
    public IEnumerable<NotificationDto> NotificationsList { get; set; } = Enumerable.Empty<NotificationDto>();
    
    public DateTime? LastCreatedAt { get; private set; }
    public const int PageSize = 8; // Default page size
    
    
    public async Task<IActionResult> OnGet(CancellationToken ct = default)
    {
        await OnGetPaged(null, ct);
        return Page();
    }
    
    public async Task<IActionResult> OnGetPaged(DateTime? lastCreatedAt, CancellationToken ct = default)
    {
        var pagedNotifications = await _notificationRepo.GetNotificationsDtoAsync(_currentUser.ID, lastCreatedAt, PageSize, ct);
        bool hasMore = pagedNotifications.Count == PageSize; // if the count is equal to PageSize, it means there are more n available
        
        string nextUrl = hasMore
            ? $"?handler=Paged&lastCreatedAt={Uri.EscapeDataString(pagedNotifications.Last().CreatedAt.ToString("o"))}"
            : string.Empty;

        var loader = new NotificationLoaderViewModel
        {
            Notifications = pagedNotifications,
            NextUrl = nextUrl,
            HasMore = hasMore,
        };
        
        return Partial("Shared/Account/Notifications/_NotificationLoaderPartial", loader);
    }
    
    public async Task<IActionResult> OnGetUserNotificationCount(CancellationToken ct = default)
    {
        var notifications = await _notificationRepo.GetUserNotificationCount(_currentUser.ID, ct);
        
        return new JsonResult(new
        {
            count = notifications
        });
    }
    
    public async Task<IActionResult> OnGetUserFollowIds(CancellationToken ct = default)
    {
        var followIds = await _notificationRepo.GetUserFollowIds(_currentUser.ID, ct);
        return new JsonResult(followIds);
    }
    
    public async Task<IActionResult> OnPostDeleteNotification(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _notificationRepo.GetNotificationAsync(notificationId, ct);
        if (notification is null)
            return NotFound();
       
        if (notification.UserID != _currentUser.ID)
            return Forbid();
        
        await _notificationRepo.DeleteAndSaveAsync(notification, ct);

        return Page();
    }
}