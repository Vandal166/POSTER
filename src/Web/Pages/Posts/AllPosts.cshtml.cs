using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;

public class AllPosts : PageModel
{
    private readonly IPostRepository _postRepository;

    public IEnumerable<PostDto> Posts { get; private set; } = Enumerable.Empty<PostDto>();

    public AllPosts(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task OnGetAsync()
    {
        // Example: page 1, pageSize 20
        var pagedPosts = await _postRepository.GetAllAsync(1, 20);
        Posts = pagedPosts.Items;
    }
}