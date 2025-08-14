using Application.DTOs;
using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface IConversationMessageRepository
{
    Task<bool> ExistsAsync(Guid conversationID, Guid messageId, CancellationToken ct = default);
    
    Task<List<MessageDto>> GetMessagesByConversationAsync(Guid conversationID, Guid requestingUserID, DateTime? lastMessageAt, int pageSize, CancellationToken ct = default);
    Task<Message?> GetMessageAsync(Guid conversationID, Guid messageID, CancellationToken ct = default);
    Task<MessageDto?> GetMessageDtoAsync(Guid conversationID, Guid messageID, CancellationToken ct = default);
    
    Task AddAsync(Message message, CancellationToken ct = default);
    Task UpdateAsync(Message message, CancellationToken ct = default);
    Task DeleteAsync(Message message, CancellationToken ct = default);
}