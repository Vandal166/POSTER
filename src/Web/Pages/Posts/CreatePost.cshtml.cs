using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages.Posts;

[Authorize]
public class CreatePost : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostService _postService;
    private readonly INotificationRepository _notificationRepo;
    private readonly IFollowNotifier _followNotifier;

    public CreatePost(ICurrentUserService currentUser, IPostService postService, INotificationRepository notificationRepo, IFollowNotifier followNotifier)
    {
        _currentUser = currentUser;
        _postService = postService;
        _notificationRepo = notificationRepo;
        _followNotifier = followNotifier;
    }
    
    public IActionResult OnGet() => RedirectToPage("/Index");
    
    [BindProperty]
    public CreatePostDto PostDto { get; set; }
    
    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return Partial("Shared/Posts/_CreatePostFormPartial", PostDto);
        }

        var result = await _postService.CreatePostAsync(PostDto, _currentUser.ID, ct);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
            return Partial("Shared/Posts/_CreatePostFormPartial", PostDto)
                .WithHxToast(Response.HttpContext, "Error creating post", "error");
        }
        
        var post = await _postService.GetPostAsync(result.Value, ct);
        
        var followIds = await _notificationRepo.GetUserFollowersIds(post!.AuthorID, ct);
        await _followNotifier.NotifyPostCreatedAsync(post.ID, followIds, ct);
        await _notificationRepo.AddRangeAndSaveAsync(followIds.Select(follower => 
            Notification.Create(follower, $"A new post has been created by {post.Author.Username}.", 
                $"/Posts/Details/{post.ID}").Value), ct);
        
        Response.Headers["HX-Redirect"] = Url.Page("/Index");
        return new EmptyResult();
    }
}