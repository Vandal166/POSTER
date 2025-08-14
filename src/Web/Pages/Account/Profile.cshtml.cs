using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages;

[AllowAnonymous, RedirectIncompleteUserProfile]
public class Profile : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepo;
    private readonly IFollowService _followService;
    private readonly IPostRepository _postRepository;
    private readonly IPostLikeRepository _postLikeRepo;
    private readonly IPostCommentRepository _postCommentRepo;
    private readonly IPostViewRepository _postViewRepo;
    
    [BindProperty]
    public UserProfileLoaderViewModel UserProfileLoader { get; set; } = new UserProfileLoaderViewModel();
    public DateTime? LastCreatedAt { get; private set; }
    public const int PageSize = 6; // Default page size for posts
  
    public Profile(ICurrentUserService currentUser, IUserRepository userRepo, IFollowService followService,
        IPostRepository postRepository, IPostLikeRepository postLikeRepo, IPostCommentRepository postCommentRepo,
        IPostViewRepository postViewRepo)
    {
        _currentUser = currentUser;
        _userRepo = userRepo;
        _followService = followService;
        _postRepository = postRepository;
        _postLikeRepo = postLikeRepo;
        _postCommentRepo = postCommentRepo;
        _postViewRepo = postViewRepo;
    }

    // id is userID
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepo.GetUserProfileDtoAsync(id, ct);
        if(user is null)
            return NotFound();

        UserProfileLoader = new UserProfileLoaderViewModel
        {
            User = user,
            Followers = await _followService.GetFollowersAsync(id, ct),
            Following = await _followService.GetFollowingAsync(id, ct),
            IsFollowing = await _followService.IsFollowingAsync(_currentUser.ID, id, ct),
        };
        await OnGetPagedAsync(id, null, ct);
        
        //returning the partial view?
        return Page();
    }
    
    public async Task<IActionResult> OnGetPagedAsync(Guid id, DateTime? lastCreatedAt, CancellationToken ct = default)
    {
        var pagedPosts = await _postRepository.GetUserFeedAsync(id, lastCreatedAt, PageSize, ct);
        bool hasMore = pagedPosts.Count == PageSize; // if the count is equal to PageSize, it means there are more posts available
        
        string nextUrl = hasMore
            ? $"?handler=Paged&lastCreatedAt={Uri.EscapeDataString(pagedPosts.Last().CreatedAt.ToString("o"))}"
            : string.Empty;
        
        var postDtos = pagedPosts.Select(p =>
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
        
        UserProfileLoader.UserPosts = new PostLoaderViewModel
        {
            Posts = aggregates,
            HasMore = hasMore,
            NextUrl = nextUrl
        };
        
        return Partial("Shared/Posts/_PostLoaderPartial", UserProfileLoader.UserPosts);
    }
}