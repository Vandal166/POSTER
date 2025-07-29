using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
            return Partial("_CreatePostPartial", this);
        }

        var result = await _postService.CreatePostAsync(PostDto, _currentUser.ID, ct);

        if (!result.IsSuccess)
        {
            return RedirectToPage("/Index"); //TODO display an floating error
        }

        return RedirectToPage("/Index"); // Redirect to the index page after creating a post
    }
}