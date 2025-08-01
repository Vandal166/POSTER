using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;
public class ViewPostRequest
{
    public Guid postID { get; set; }
}
[Authorize, IgnoreAntiforgeryToken]
public class ViewPost : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostRepository _postRepo;
    private readonly IPostCommentRepository _postCommentRepo;
    private readonly IPostLikeRepository _postLikeRepo;
    private readonly IPostViewService _postViewService;

    public ViewPost(ICurrentUserService currentUser, IPostRepository postRepo, IPostCommentRepository postCommentRepo, IPostLikeRepository postLikeRepo,
    IPostViewService postViewService)
    {
        _currentUser = currentUser;
        _postRepo = postRepo;
        _postCommentRepo = postCommentRepo;
        _postLikeRepo = postLikeRepo;
        _postViewService = postViewService;
    }
    
    public async Task<IActionResult> OnGetAsync(Guid postId, CancellationToken ct = default)
    {
        var post = await _postRepo.GetPostAsync(postId, ct);
        if (post is null)
            return NotFound();
        
        var vm = new PostAggregateDto
        {
            Post = post,
            LikeCount = await _postLikeRepo.GetLikesCountByPostAsync(post.Id, ct),
            CommentCount = await _postCommentRepo.GetCommentsCountByPostAsync(post.Id, ct),
            ViewCount = await _postViewService.GetViewsCountByPostAsync(post.Id, ct),
            IsLiked = await _postLikeRepo.IsPostLikedByUserAsync(post.Id, _currentUser.ID, ct)
        };
        
        return Partial("Shared/Posts/_PostStatsPartial", vm);
    }

    public async Task<IActionResult> OnPostAsync([FromBody] ViewPostRequest? request, CancellationToken ct = default)
    {
        if (request is null || request.postID == Guid.Empty)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Invalid post ID."
            });
        }

        if (await _postViewService.AddViewAsync(request.postID, _currentUser.ID, ct) == false)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Failed to record post view. The post may not exist or you have already viewed it."
            });
        }

        return new JsonResult(new
        {
            success = true,
            message = "Post view recorded successfully."
        });
    }
}