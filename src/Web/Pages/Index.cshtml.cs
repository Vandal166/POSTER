using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostRepository _postRepository;
    private readonly IPostLikeRepository _postLikeRepo;
    private readonly IPostCommentRepository _postCommentRepo;
    private readonly IPostViewRepository _postViewRepo;
    
    public IEnumerable<PostAggregateDto> Posts { get; private set; } = Enumerable.Empty<PostAggregateDto>();
    
    public int CurrentPage { get; private set; }
    public const int PageSize = 4; // Default page size
    
    public IndexModel(ICurrentUserService currentUser, IPostRepository postRepository, IPostLikeRepository postLikeRepo, IPostCommentRepository postCommentRepo, 
        IPostViewRepository postViewRepo)
    {
        _currentUser = currentUser;
        _postRepository = postRepository;
        _postLikeRepo = postLikeRepo;
        _postCommentRepo = postCommentRepo;
        _postViewRepo = postViewRepo;
    }


    public async Task<IActionResult> OnGet(CancellationToken ct = default)
    {
        var user = HttpContext.User;
        
        if(user?.Identity?.IsAuthenticated != false && _currentUser.HasClaim("profileCompleted", "false"))
            return RedirectToPage("/Account/CompleteProfile");

        await OnGetPaged(1, ct);
        return Page();
    }
    
    public async Task<IActionResult> OnGetPaged(int pageNumber = 1, CancellationToken ct = default)
    {
        var pagedPosts = await _postRepository.GetAllAsync(pageNumber, PageSize, ct);
        bool hasMore = pagedPosts.HasNextPage;
        string nextUrl = hasMore ? $"?handler=Paged&pageNumber={pageNumber + 1}" : string.Empty;
        
        var postDtos = pagedPosts.Items.Select(p =>
            p with
            {
                Content = (p.IsTruncated ? string.Concat(p.Content.AsSpan(0, 300), "...") : p.Content)
            }).ToList();

        var aggregates = new List<PostAggregateDto>(postDtos.Count);
        foreach (var post in postDtos)
        {
            aggregates.Add(new PostAggregateDto
            {
                Post = post,
                LikeCount = await _postLikeRepo.GetLikesCountByPostAsync(post.Id, ct),
                CommentCount = await _postCommentRepo.GetCommentsCountByPostAsync(post.Id, ct),
                ViewCount = await _postViewRepo.GetViewsCountByPostAsync(post.Id, ct),
                IsLiked = await _postLikeRepo.IsPostLikedByUserAsync(post.Id, _currentUser.ID, ct)
            });
        }
        
        var vm = new PostLoaderViewModel
        {
            Posts = aggregates,
            HasMore = hasMore,
            NextUrl = nextUrl
        };
        
        return Partial("_PostLoaderPartial", vm);
    }
}