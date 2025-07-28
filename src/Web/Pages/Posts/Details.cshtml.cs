using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;

public class Details : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostRepository _postRepository;
    private readonly IPostCommentRepository _postCommentRepo;
    private readonly IPostLikeRepository _postLikeRepo;
    private readonly IPostViewRepository _postViewRepo;
    
    public PostAggregateDto? Post { get; private set; } = null!;
    public IEnumerable<CommentDto> Comments { get; private set; } = Enumerable.Empty<CommentDto>();

    public Details(ICurrentUserService currentUser, IPostRepository postRepository, IPostCommentRepository commentRepository, IPostLikeRepository postLikeRepo,
        IPostCommentRepository postCommentRepo, IPostViewRepository postViewRepo)
    {
        _currentUser = currentUser;
        _postRepository = postRepository;
        _postCommentRepo = commentRepository;
        _postLikeRepo = postLikeRepo;
        _postViewRepo = postViewRepo;
    }

    public async Task<IActionResult> OnGetAsync(Guid id, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var post = await _postRepository.GetPostAsync(id, ct);
        if (post is null)
            return NotFound();
        
        Post = new PostAggregateDto
        {
            Post = post,
            LikeCount = await _postLikeRepo.GetLikesCountByPostAsync(id, ct),
            CommentCount = await _postCommentRepo.GetCommentsCountByPostAsync(id, ct),
            ViewCount = await _postViewRepo.GetViewsCountByPostAsync(id, ct),
            IsLiked = await _postLikeRepo.IsPostLikedByUserAsync(post.Id, _currentUser.ID, ct)
        };
        var pagedComments = await _postCommentRepo.GetCommentsByPostAsync(id, page, pageSize, ct);
        Comments = pagedComments.Items;

        return Page();
    }
}