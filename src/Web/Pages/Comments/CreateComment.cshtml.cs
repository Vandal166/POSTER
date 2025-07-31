using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;

[Authorize]
public class CreateComment : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostCommentService _commentService;

    public CreateComment(ICurrentUserService currentUser, IPostCommentService commentService)
    {
        _currentUser = currentUser;
        _commentService = commentService;
    }
    
    public IActionResult OnGet(Guid postId) => RedirectToPage("/Index");
    
    [BindProperty]
    public CreateCommentDto CommentDto { get; set; }
    
    // this is for creating an comment on a post
    public async Task<IActionResult> OnPostCommentOnPostAsync(Guid postId, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = new CreateCommentViewModel
            {
                EntityId = postId,
                Content = CommentDto.Content
            };
            return Partial("_CommentOnPostForm", viewModel);
        }
        
        var result = await _commentService.CreateCommentAsync(postId, _currentUser.ID, CommentDto, ct);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            var viewModel = new CreateCommentViewModel
            {
                EntityId = postId,
                Content = CommentDto.Content
            };
            return Partial("_CommentOnPostForm", viewModel);
        }

        // HTMX will look for this and trigger a client-side redirect
        Response.Headers["HX-Redirect"] = Url.Page("/Posts/Details", new { id = postId });
        return new EmptyResult();
    }
    
    // this is for creating an comment on a comment
    public async Task<IActionResult> OnPostCommentOnCommentAsync(Guid postId, Guid parentCommentId, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = new CreateCommentViewModel
            {
                EntityId = postId,
                ParentCommentId = parentCommentId,
                Content = CommentDto.Content
            };
            return Partial("_CommentOnCommentForm", viewModel);
        }
        
        var result = await _commentService.CreateCommentOnCommentAsync(postId, parentCommentId,_currentUser.ID, CommentDto, ct);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            var viewModel = new CreateCommentViewModel
            {
                EntityId = postId,
                ParentCommentId = parentCommentId,
                Content = CommentDto.Content
            };
            return Partial("_CommentOnCommentForm", viewModel);
        }

        // HTMX will look for this and trigger a client-side redirect
        Response.Headers["HX-Redirect"] = Url.Page("/Comments/Details", new { id = parentCommentId });
        return new EmptyResult();
    }
}