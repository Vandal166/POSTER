using Application.DTOs;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IConversationService
{
    Task<Result<Guid>> CreateConversationAsync(Guid currentUserID, List<Guid>? participantIDs, CreateConversationDto dto, CancellationToken ct = default);
    
    Task<Result<bool>> UpdateConversationAsync(UpdateConversationDto dto, Guid currentUserID, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteConversationAsync(Guid conversationID, Guid currentUserID, CancellationToken cancellationToken = default);
    
    Task<Result<bool>> AddParticipantAsync(Guid conversationID, List<Guid>? participantIDs, Guid currentUserID, CancellationToken ct = default);
    Task<Result<bool>> LeaveConversationAsync(Guid conversationID, Guid currentUserID, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoveParticipantAsync(Guid conversationID, Guid participantID, Guid currentUserID, CancellationToken ct = default);
}