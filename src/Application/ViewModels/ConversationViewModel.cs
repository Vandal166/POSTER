using Application.DTOs;

namespace Application.ViewModels;

public sealed class ConversationViewModel
{
    public ConversationDto Conversation { get; set; } = null!;
    public IEnumerable<UserDto> Participants { get; set; } = Enumerable.Empty<UserDto>();
}