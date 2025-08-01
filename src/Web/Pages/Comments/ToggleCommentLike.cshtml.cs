using Application.Contracts;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;

[Authorize] // only authenticated users can like posts
public class LikeCommentModel : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICommentLikeService _likeService;

    public LikeCommentModel(ICurrentUserService currentUser, ICommentLikeService likeService)
    {
        _currentUser = currentUser;
        _likeService = likeService;
    }
    

    public async Task<IActionResult> OnPostToggleLikeAsync(Guid commentId, CancellationToken ct = default)
    {
        var vm = await _likeService.ToggleLikeAsync(commentId, _currentUser.ID, ct);

        return Partial("Shared/Comments/_CommentLikesPartial", vm);
    }
}