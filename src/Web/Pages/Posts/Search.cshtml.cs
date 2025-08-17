using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;

public class Search : PageModel
{
    private readonly IPostRepository _postRepository;
    private readonly IPostLikeRepository _postLikeRepo;
    private readonly IPostCommentRepository _postCommentRepo;
    private readonly IPostViewRepository _postViewRepo;
    private readonly ICurrentUserService _currentUser;
    
    public string SearchQuery { get; set; } = string.Empty;
    public IEnumerable<PostAggregateDto> SearchResults { get; private set; } = Enumerable.Empty<PostAggregateDto>();
    
    public Search(IPostRepository postRepository, IPostLikeRepository postLikeRepo, IPostCommentRepository postCommentRepo,
        IPostViewRepository postViewRepo, ICurrentUserService currentUser)
    {
        _postRepository = postRepository;
        _postLikeRepo = postLikeRepo;
        _postCommentRepo = postCommentRepo;
        _postViewRepo = postViewRepo;
        _currentUser = currentUser;
    }
    
    public async Task<IActionResult> OnGet(string q, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Page();
        
        SearchQuery = q;
        var posts = await _postRepository.SearchPostsAsync(q, ct);
        
        posts = posts.Select(p =>
            p with
            {
                Content = (p.IsTruncated ? string.Concat(p.Content.AsSpan(0, 300), "...") : p.Content)
            }).ToList();
        
        var aggregates = new List<PostAggregateDto>(posts.Count);
        foreach (var post in posts)
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
        
        SearchResults = aggregates;
        return Page();
    }
}