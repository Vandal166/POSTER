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
    
    public async Task<IActionResult> OnPostCommentAsync(Guid postId, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = new CreateCommentViewModel
            {
                PostId = postId,
                Content = CommentDto.Content
            };
            return Partial("_CommentForm", viewModel);
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
                PostId = postId,
                Content = CommentDto.Content
            };
            return Partial("_CommentForm", viewModel);
        }

        // HTMX will look for this and trigger a client-side redirect
        Response.Headers["HX-Redirect"] = Url.Page("/Posts/Details", new { id = postId });
        return new EmptyResult();
    }
}