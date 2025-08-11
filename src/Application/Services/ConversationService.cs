using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using FluentValidation;

namespace Application.Services;

public sealed class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversations;
    private readonly IValidator<CreateConversationDto> _createConversationValidator;
    private readonly IUnitOfWork _uow;
    private readonly IBlobService _blobService;
    private readonly IConversationNotifier _conversationNotifier;
    
    public ConversationService(IConversationRepository conversations, IValidator<CreateConversationDto> createConversationValidator, IUnitOfWork uow, 
        IBlobService blobService, IConversationNotifier conversationNotifier)
    {
        _conversations = conversations;
        _createConversationValidator = createConversationValidator;
        _uow = uow;
        _blobService = blobService;
        _conversationNotifier = conversationNotifier;
    }

    
    public async Task<Result<Guid>> CreateConversationAsync(Guid currentUserID, List<Guid>? participantIDs, CreateConversationDto dto, CancellationToken ct = default)
    {
        if (participantIDs is null || participantIDs.Count < 2)
            return Result.Fail<Guid>("At least two participants are required to create a conversation.");
        
        var validation = await _createConversationValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail<Guid>(validation.Errors.Select(e => e.ErrorMessage));
        
        var conversation = Conversation.Create(dto.Name, dto.ProfilePictureFileID, currentUserID);
        if (conversation.IsFailed)
            return Result.Fail<Guid>(conversation.Errors.Select(e => e.Message));
        
        await _conversations.AddAsync(conversation.Value, ct);
        //TODO upon creating, add an default Message impersonating the creator so that the covnersation.Message is correctly indexed via CreatedAt and shown up top
        
        var conversationUsers = participantIDs.Select(id => new ConversationUser
        {
            ConversationID = conversation.Value.ID,
            UserID = id,
            JoinedAt = DateTime.UtcNow
        }).ToList();

        foreach (var user in conversationUsers)
        {
            await _conversations.AddParticipantsAsync(user, ct);
        }
        await _uow.SaveChangesAsync(ct);
        
        await _conversationNotifier.NotifyConversationCreatedAsync(conversation.Value.ID, participantIDs, ct);
        
        return Result.Ok(conversation.Value.ID);
    }

    public async Task<IPagedList<ConversationDto>> GetAllAsync(Guid currentUserID, DateTime? lastMessageAt, int pageSize,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ConversationDto?> GetConversationAsync(Guid conversationID, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> DeleteConversationAsync(Guid conversationID, Guid currentUserID, CancellationToken ct = default)
    {
        if (await _conversations.ExistsAsync(conversationID, ct) is false)
            return Result.Fail<bool>("Conversation does not exist.");
        
        var conversation = await _conversations.GetConversationAsync(conversationID, currentUserID, ct);
        if (conversation is null)
            return Result.Fail<bool>("Conversation not found or you are not part of it.");
        
        if (conversation.CreatedByID != currentUserID)
            return Result.Fail<bool>("You can only delete conversations you created.");
        
        await _blobService.DeleteFileAsync(conversation.ProfilePictureID, "images", ct);
        
        await _conversations.DeleteAsync(conversation, ct);
        await _uow.SaveChangesAsync(ct);
        
        return Result.Ok(true);
    }
    
    //TODO LeaveConversationAsync ??
}