using Application.DTOs;

namespace Application.ViewModels;

public sealed class PostLoaderViewModel
{
    public IEnumerable<PostAggregateDto> Posts { get; set; }
    public string NextUrl { get; set; } //setting the url for the next page of posts, if empty, there are no more posts
    public bool HasMore { get; set; }
}