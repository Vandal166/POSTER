using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;

[Authorize] // only authenticated users can like posts
public class LikePostModel : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostLikeService _likeService;
    private readonly IPostViewService _postViewService;

    public LikePostModel(ICurrentUserService currentUser, IPostLikeService likeService, IPostViewService postViewService)
    {
        _currentUser = currentUser;
        _likeService = likeService;
        _postViewService = postViewService;
    }
    

    public async Task<IActionResult> OnPostToggleLikeAsync(Guid postId, CancellationToken ct = default)
    {
        var vm = await _likeService.ToggleLikeAsync(postId, _currentUser.ID, ct);

        await _postViewService.AddViewAsync(postId, _currentUser.ID, ct);
        
        return Partial("Shared/Posts/_PostLikesPartial", vm);
    }
}