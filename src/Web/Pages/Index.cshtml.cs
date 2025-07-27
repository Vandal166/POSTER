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
    
    
    public IEnumerable<PostDto> Posts { get; private set; } = Enumerable.Empty<PostDto>();

    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    
    public IndexModel(ICurrentUserService currentUser, IPostRepository postRepository)
    {
        _currentUser = currentUser;
        _postRepository = postRepository;
    }
    
    public async Task<IActionResult> OnGet(int pageNumber = 1, int pageSize = 10)
    {
        var user = HttpContext.User;
        
        if(user?.Identity?.IsAuthenticated != false && _currentUser.HasClaim("profileCompleted", "false"))
            return RedirectToPage("/Account/CompleteProfile");
        
        var pagedPosts = await _postRepository.GetAllAsync(pageNumber, pageSize);
        Posts = pagedPosts.Items.Select(p =>
            p with
            {
                Content = (p.Content?.Length > 300 ? string.Concat(p.Content.AsSpan(0, 300), "... Click expand post") : p.Content)!
            });
        
        //TODO add better way to handle content truncation
        CurrentPage = pagedPosts.Page;
        TotalPages = pagedPosts.TotalCount;
        
        return Page();
    }
}