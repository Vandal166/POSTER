using Application.DTOs;

namespace Application.ViewModels;

public sealed class MessageLoaderViewModel
{
    public IEnumerable<MessageDto> Messages { get; set; }
    public string NextUrl { get; set; } //setting the url for the next page of messages
    public bool HasMore { get; set; }
}