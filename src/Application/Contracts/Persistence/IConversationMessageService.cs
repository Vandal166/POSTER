using Application.DTOs;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IConversationMessageService
{
    Task<Result<Guid>> CreateMessageAsync(Guid currentUserID, CreateMessageDto dto, CancellationToken ct = default);
    Task<Result<Guid>> CreateSystemMessageAsync(CreateMessageDto dto, CancellationToken ct = default);
    
    Task<IPagedList<MessageDto>> GetMessagesByConversationAsync(Guid conversationID, Guid currentUserID, DateTime? firstMessageAt, int pageSize, CancellationToken ct = default);
    
    Task<MessageDto?> GetMessageAsync(Guid messageID, CancellationToken ct = default);
    
    Task<Result<bool>> DeleteMessageAsync(Guid conversationID, Guid messageID, Guid currentUserID, CancellationToken ct = default);
}