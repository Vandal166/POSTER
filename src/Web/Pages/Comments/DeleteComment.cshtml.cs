using Application.Contracts;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages.Posts;

[Authorize]
public class DeleteComment : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostCommentService _commentService;
    private readonly IToastBuilder _toastBuilder;


    public DeleteComment(ICurrentUserService currentUser, IPostCommentService commentService, IToastBuilder toastBuilder)
    {
        _currentUser = currentUser;
        _commentService = commentService;
        _toastBuilder = toastBuilder;
    }
    
    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(Guid commentId, CancellationToken ct = default)
    {
        var result = await _commentService.DeleteCommentAsync(commentId, _currentUser.ID, ct);
        
        _toastBuilder.SetToast(result)
            .OnSuccess("Comment deleted successfully").Build(TempData);
        
        return RedirectToPage("/Index");
    }
}