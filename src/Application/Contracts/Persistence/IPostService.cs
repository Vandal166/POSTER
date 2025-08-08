using Application.DTOs;
using Domain.Entities;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IPostService
{
    Task<Result<Guid>> CreatePostAsync(CreatePostDto dto, Guid userID, CancellationToken cancellationToken);

    Task<IPagedList<PostDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeletePostAsync(Guid id, Guid currentUserID, CancellationToken cancellationToken = default);
}

public interface IConversationService
{
    Task<Result<Guid>> CreateConversationAsync(Guid currentUserID, List<Guid>? participantIDs, CreateConversationDto dto, CancellationToken ct = default);
    
    Task<IPagedList<ConversationDto>> GetAllAsync(Guid currentUserID, DateTime? lastMessageAt, int pageSize, CancellationToken cancellationToken = default);
    
    Task<ConversationDto?> GetConversationAsync(Guid conversationID, CancellationToken cancellationToken = default);
    
    Task<Result<bool>> DeleteConversationAsync(Guid conversationID, Guid currentUserID, CancellationToken cancellationToken = default);
}