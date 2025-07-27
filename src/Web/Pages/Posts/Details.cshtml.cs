using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Posts;

public class Details : PageModel
{
    private readonly IPostRepository _postRepository;
    private readonly IPostCommentRepository _commentRepository;

    public PostDto? Post { get; private set; } = null!;
    public IEnumerable<CommentDto> Comments { get; private set; } = Enumerable.Empty<CommentDto>();

    public Details(IPostRepository postRepository, IPostCommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    public async Task<IActionResult> OnGetAsync(Guid id, int page = 1, int pageSize = 20)
    {
        Post = await _postRepository.GetPostAsync(id);
        if (Post == null)
            return NotFound();
        

        var pagedComments = await _commentRepository.GetCommentsByPostAsync(id, page, pageSize);
        Comments = pagedComments.Items;

        return Page();
    }
}