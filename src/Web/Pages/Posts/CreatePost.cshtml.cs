using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages.Posts;

[Authorize]
public class CreatePost : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPostService _postService;

    public CreatePost(ICurrentUserService currentUser, IPostService postService)
    {
        _currentUser = currentUser;
        _postService = postService;
    }
    
    public IActionResult OnGet() => RedirectToPage("/Index");
    
    [BindProperty]
    public CreatePostDto PostDto { get; set; }
    
    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return Partial("Shared/Posts/_CreatePostFormPartial", PostDto);
        }

        var result = await _postService.CreatePostAsync(PostDto, _currentUser.ID, ct);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
            return Partial("Shared/Posts/_CreatePostFormPartial", PostDto)
                .WithHxToast(Response.HttpContext, "Error creating post", "error");
        }

        Response.Headers["HX-Redirect"] = Url.Page("/Index");
        return new EmptyResult();
    }
}