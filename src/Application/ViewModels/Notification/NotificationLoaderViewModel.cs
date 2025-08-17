using Application.DTOs;

namespace Application.ViewModels;

public sealed class NotificationLoaderViewModel
{
    public IEnumerable<NotificationDto> Notifications { get; set; }
    public string NextUrl { get; set; } //setting the url for the next page of posts, if empty, there are no more posts
    public bool HasMore { get; set; }
}