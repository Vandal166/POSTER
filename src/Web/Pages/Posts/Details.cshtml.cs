using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages.Posts;

[AllowAnonymous, RedirectIncompleteUserProfile]
public class Details : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostRepository _postRepository;
    private readonly IPostCommentRepository _postCommentRepo;
    private readonly IPostLikeRepository _postLikeRepo;
    private readonly ICommentLikeRepository _commentLikeRepo;
    private readonly IPostViewService _postViewService;
    
    public PostAggregateDto? Post { get; private set; } = null!;
    public IEnumerable<CommentAggregateDto> Comments { get; private set; } = Enumerable.Empty<CommentAggregateDto>();
    
    public int CurrentPage { get; private set; }
    public const int PageSize = 6; // Default page size
    
    public Details(ICurrentUserService currentUser, IPostRepository postRepository, IPostCommentRepository commentRepository, IPostLikeRepository postLikeRepo,
        IPostCommentRepository postCommentRepo, ICommentLikeRepository commentLikeRepo, IPostViewService postViewService)
    {
        _currentUser = currentUser;
        _postRepository = postRepository;
        _postCommentRepo = commentRepository;
        _postLikeRepo = postLikeRepo;
        _commentLikeRepo = commentLikeRepo;
        _postViewService = postViewService;
    }

    // id is postID
    public async Task<IActionResult> OnGetAsync(Guid id, int pageNumber = 1, CancellationToken ct = default)
    {
        var post = await _postRepository.GetPostAsync(id, ct);
        if (post is null)
            return NotFound();
        
        await _postViewService.AddViewAsync(id, _currentUser.ID, ct);
        
        Post = new PostAggregateDto
        {
            Post = post,
            LikeCount = await _postLikeRepo.GetLikesCountByPostAsync(id, ct),
            CommentCount = await _postCommentRepo.GetCommentsCountByPostAsync(id, ct),
            ViewCount = await _postViewService.GetViewsCountByPostAsync(id, ct),
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

        var aggregates = new List<CommentAggregateDto>(pagedComments.Items.Count);
        foreach (var comment in pagedComments.Items)
        {
            aggregates.Add(new CommentAggregateDto
            {
                Comment = comment,
                PostId = id,
                LikeCount = await _commentLikeRepo.GetLikesCountByCommentAsync(comment.Id, ct),
                CommentCount = await _postCommentRepo.GetCommentsCountByCommentAsync(comment.Id, ct),
                IsLiked = await _commentLikeRepo.IsCommentLikedByUserAsync(comment.Id, _currentUser.ID, ct)
            });
        }
        
        var vm = new CommentLoaderViewModel
        {
            Comments = aggregates,
            HasMore = hasMore,
            NextUrl = nextUrl
        };

        return Partial("Shared/Comments/_CommentLoaderPartial", vm);
    }
}