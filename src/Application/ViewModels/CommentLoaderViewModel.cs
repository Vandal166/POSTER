using Application.DTOs;

namespace Application.ViewModels;

// ViewModel for loading comments in a paginated manner as well providing the next URL for HTMX
public class CommentLoaderViewModel
{
    public IEnumerable<CommentDto> Comments { get; set; }
    public string NextUrl { get; set; } //setting the url for the next page of comments
    public bool HasMore { get; set; }
}