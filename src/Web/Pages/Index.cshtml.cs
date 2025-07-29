using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
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
    public int TotalPages { get; private set; }
    public const int PageSize = 4; // Default page size
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
    
    public IndexModel(ICurrentUserService currentUser, IPostRepository postRepository, IPostLikeRepository postLikeRepo, IPostCommentRepository postCommentRepo, 
        IPostViewRepository postViewRepo)
    {
        _currentUser = currentUser;
        _postRepository = postRepository;
        _postLikeRepo = postLikeRepo;
        _postCommentRepo = postCommentRepo;
        _postViewRepo = postViewRepo;
    }
    
    public async Task<IActionResult> OnGet(int pageNumber = 1, CancellationToken ct = default)
    {
        var user = HttpContext.User;
        
        if(user?.Identity?.IsAuthenticated != false && _currentUser.HasClaim("profileCompleted", "false"))
            return RedirectToPage("/Account/CompleteProfile");
        
        var pagedPosts = await _postRepository.GetAllAsync(pageNumber, PageSize, ct);
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
        
        Posts = aggregates;
        CurrentPage = pagedPosts.Page;
        TotalPages = pagedPosts.TotalCount;
        
        return Page();
    }
}