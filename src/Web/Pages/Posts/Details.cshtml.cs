using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
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
    
    public int CurrentPage { get; private set; }
    public const int PageSize = 6; // Default page size
    
    public Details(ICurrentUserService currentUser, IPostRepository postRepository, IPostCommentRepository commentRepository, IPostLikeRepository postLikeRepo,
        IPostCommentRepository postCommentRepo, IPostViewRepository postViewRepo)
    {
        _currentUser = currentUser;
        _postRepository = postRepository;
        _postCommentRepo = commentRepository;
        _postLikeRepo = postLikeRepo;
        _postViewRepo = postViewRepo;
    }

    public async Task<IActionResult> OnGetAsync(Guid id, int pageNumber = 1, CancellationToken ct = default)
    {
        var user = HttpContext.User;
        
        if(user?.Identity?.IsAuthenticated != false && _currentUser.HasClaim("profileCompleted", "false"))
            return RedirectToPage("/Account/CompleteProfile");

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
        
        await OnGetPaged(id, pageNumber, ct);
        
        return Page();
    }
    
    public async Task<IActionResult> OnGetPaged(Guid id, int pageNumber = 1, CancellationToken ct = default)
    {
        var pagedComments = await _postCommentRepo.GetCommentsByPostAsync(id, pageNumber, PageSize, ct);
        bool hasMore = pagedComments.HasNextPage;

        string nextUrl = hasMore ? $"?handler=Paged&id={id}&pageNumber={pageNumber + 1}" : string.Empty;

        var model = new CommentLoaderViewModel
        {
            Comments = pagedComments.Items,
            HasMore = hasMore,
            NextUrl = nextUrl
        };

        return Partial("_CommentLoaderPartial", model);
    }
}