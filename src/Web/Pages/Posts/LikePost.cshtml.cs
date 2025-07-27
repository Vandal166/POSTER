using Application.Contracts;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;

[Authorize] // only authenticated users can like posts
public class LikePostModel : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostRepository _postRepository;
    private readonly IPostLikeService _likeService;

    public LikePostModel(ICurrentUserService currentUser, IPostRepository postRepository, IPostLikeService likeService)
    {
        _currentUser = currentUser;
        _postRepository = postRepository;
        _likeService = likeService;
    }


    public async Task<IActionResult> OnPostLikeAsync(Guid postId, CancellationToken ct)
    {
        var result = await _likeService.LikePostAsync(postId, Guid.Parse(_currentUser.ID), ct);

        if (!result.IsSuccess)
            return BadRequest();

        var updatedPost = await _postRepository.GetPostAsync(postId, ct);
        return Partial("_PostLikesPartial", updatedPost);
    }
}