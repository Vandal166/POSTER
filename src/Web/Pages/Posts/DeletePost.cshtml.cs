using Application.Contracts;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages.Posts;

[Authorize]
public class DeletePost : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostService _postService;
    private readonly IToastBuilder _toastBuilder;


    public DeletePost(ICurrentUserService currentUser, IPostService postService, IToastBuilder toastBuilder)
    {
        _currentUser = currentUser;
        _postService = postService;
        _toastBuilder = toastBuilder;
    }
    
    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(Guid postId, CancellationToken ct = default)
    {
        var result = await _postService.DeletePostAsync(postId, _currentUser.ID, ct);
        
        _toastBuilder.SetToast(result)
            .OnSuccess("Post deleted successfully").Build(TempData);
        
        return RedirectToPage("/Index");
    }
}