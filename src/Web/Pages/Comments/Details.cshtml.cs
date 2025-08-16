using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages.Comments;

[AllowAnonymous, RedirectIncompleteUserProfile]
public class Details : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostCommentRepository _commentRepo;
    private readonly ICommentLikeRepository _commentLikeRepo;
    private readonly IPostRepository _postRepo;
    
    
    public CommentAggregateDto? Comment { get; private set; } = null!;
    public IEnumerable<CommentAggregateDto> Comments { get; private set; } = Enumerable.Empty<CommentAggregateDto>();
    
    public int CurrentPage { get; private set; }
    public const int PageSize = 6; // Default page size
    
    public Details(ICurrentUserService currentUser, IPostCommentRepository commentRepo, ICommentLikeRepository commentLikeRepo, IPostRepository postRepo)
    {
        _currentUser = currentUser;
        _commentRepo = commentRepo;
        _commentLikeRepo = commentLikeRepo;
        _postRepo = postRepo;
    }

    // id is commentID
    public async Task<IActionResult> OnGetAsync(Guid id, int pageNumber = 1, CancellationToken ct = default)
    {
        var comment = await _commentRepo.GetCommentDtoAsync(id, ct);
        if (comment is null)
            return NotFound();
        
        var post = await _postRepo.GetPostByCommentAsync(id, ct);
        if (post is null)
            return NotFound();
        
        Comment = new CommentAggregateDto
        {
            Comment = comment,
            PostId = post.Id,
            LikeCount = await _commentLikeRepo.GetLikesCountByCommentAsync(id, ct),
            CommentCount = await _commentRepo.GetCommentsCountByCommentAsync(id, ct),
            IsLiked = await _commentLikeRepo.IsCommentLikedByUserAsync(comment.Id, _currentUser.ID, ct)
        };
        
        await OnGetPaged(id, pageNumber, ct);
        
        return Page();
    }
    
    public async Task<IActionResult> OnGetPaged(Guid id, int pageNumber = 1, CancellationToken ct = default)
    {
        var pagedComments = await _commentRepo.GetCommentsByCommentAsync(id, pageNumber, PageSize, ct);
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
                CommentCount = await _commentRepo.GetCommentsCountByCommentAsync(comment.Id, ct),
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