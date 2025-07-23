using Application.Contracts;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class Posts : PageModel
{
    private readonly IPostRepository _postRepository;

    public IEnumerable<PostDto> AllPosts { get; private set; } = Enumerable.Empty<PostDto>();

    public Posts(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task OnGetAsync()
    {
        // Example: page 1, pageSize 20
        var pagedPosts = await _postRepository.GetAllAsync(1, 20);
        AllPosts = pagedPosts.Items;
    }
}