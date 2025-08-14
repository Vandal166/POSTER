using Application.DTOs;
using Application.ViewModels;
using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface IConversationRepository
{
    Task<bool> ExistsAsync(Guid conversationId, CancellationToken ct = default);

    Task<ConversationDto?> GetConversationDtoAsync(Guid conversationId, Guid requestingUserID, CancellationToken ct = default);
    Task<Conversation?> GetConversationAsync(Guid conversationId, Guid requestingUserID, CancellationToken ct = default);
    Task<ConversationViewModel?> GetConversationViewModelAsync(Guid conversationId, Guid requestingUserID, CancellationToken ct = default);
    Task<UserDto?> GetConversationParticipantAsync(Guid conversationId, Guid participantId, CancellationToken ct = default);

    Task<IPagedList<ConversationDto>> GetAllAsync(Guid currentUserID, int pageNumber, int pageSize, CancellationToken ct = default);
    // this is just returning the IDs of all conversations the user is part of for SignalR messages notifications
    Task<List<Guid>> GetConversationsIdsAsync(Guid requestingUserID, CancellationToken ct = default);
    
    
    Task AddAsync(Conversation conversation, CancellationToken ct = default);
    Task AddParticipantsAsync(ConversationUser conversationUser, CancellationToken ct = default);
    
    Task UpdateAsync(Conversation conversation, CancellationToken ct = default);
    
    Task DeleteAsync(Conversation conversation, CancellationToken ct = default);
    Task DeleteParticipantAsync(ConversationUser conversationUser, CancellationToken ct = default);
}