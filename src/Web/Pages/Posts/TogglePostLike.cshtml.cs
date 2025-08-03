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
    private readonly IPostRepository _postRepo;
    private readonly IPostCommentRepository _postCommentRepo;
    private readonly IPostLikeService _likeService;
    private readonly IPostViewService _postViewService;

    public LikePostModel(ICurrentUserService currentUser, IPostRepository postRepo, IPostCommentRepository postCommentRepo,
        IPostLikeService likeService, IPostViewService postViewService)
    {
        _currentUser = currentUser;
        _postRepo = postRepo;
        _postCommentRepo = postCommentRepo;
        _likeService = likeService;
        _postViewService = postViewService;
    }
    
    public async Task<IActionResult> OnPostToggleLikeAsync(Guid postId, CancellationToken ct = default)
    {
        var post = await _postRepo.GetPostAsync(postId, ct);
        if (post is null)
            return NotFound();
        
        var likeResult = await _likeService.ToggleLikeAsync(postId, _currentUser.ID, ct);

        await _postViewService.AddViewAsync(postId, _currentUser.ID, ct);
        
        var vm = new PostAggregateDto
        {
            Post = post,
            LikeCount = likeResult.LikesCount,
            CommentCount = await _postCommentRepo.GetCommentsCountByPostAsync(post.Id, ct),
            ViewCount = await _postViewService.GetViewsCountByPostAsync(post.Id, ct),
            IsLiked = likeResult.IsLiked
        };
        
        return Partial("Shared/Posts/_PostStatsPartial", vm);
    }
}