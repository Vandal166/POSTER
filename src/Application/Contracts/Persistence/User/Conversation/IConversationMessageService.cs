using Application.DTOs;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IConversationMessageService
{
    Task<Result<Guid>> CreateMessageAsync(Guid currentUserID, CreateMessageDto dto, CancellationToken ct = default);
    Task<Result<Guid>> CreateSystemMessageAsync(CreateMessageDto dto, CancellationToken ct = default);
    
    Task<List<MessageDto>> GetMessagesByConversationAsync(Guid conversationID, Guid requestingUserID, DateTime? lastMessageAt, int pageSize, CancellationToken ct = default);
    
    Task<MessageDto?> GetMessageDtoAsync(Guid conversationID, Guid messageID, CancellationToken ct = default);
    
    Task<Result<bool>> DeleteMessageAsync(Guid conversationID, Guid messageID, Guid currentUserID, CancellationToken ct = default);
}