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
    private readonly IValidator<UpdateConversationDto> _updateConversationValidator;
    private readonly IUnitOfWork _uow;
    private readonly IBlobService _blobService;
    private readonly IMessageNotifier _messageNotifier;
    private readonly IConversationNotifier _conversationNotifier;
    
    public ConversationService(IConversationRepository conversations,
        IValidator<CreateConversationDto> createConversationValidator,
        IValidator<UpdateConversationDto> updateConversationValidator,
        IUnitOfWork uow, IBlobService blobService, IMessageNotifier messageNotifier, IConversationNotifier conversationNotifier)
    {
        _conversations = conversations;
        _createConversationValidator = createConversationValidator;
        _updateConversationValidator = updateConversationValidator;
        _uow = uow;
        _blobService = blobService;
        _messageNotifier = messageNotifier;
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

    public async Task<Result<bool>> UpdateConversationAsync(UpdateConversationDto dto, Guid currentUserID, CancellationToken cancellationToken = default)
    {
        var validation = await _updateConversationValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return Result.Fail<bool>(validation.Errors.Select(e => e.ErrorMessage));
        
        var conversation = await _conversations.GetConversationAsync(dto.Id, currentUserID, cancellationToken);
        if (conversation is null)
            return Result.Fail<bool>("Conversation not found or you are not part of it.");
        
        if (conversation.CreatedByID != currentUserID)
            return Result.Fail<bool>("You can only update conversations you created.");
        
        if(dto.ProfilePictureID is not null && dto.ProfilePictureID != conversation.ProfilePictureID)
        {
            // delete the old profile picture if it is not the default one
            if (conversation.ProfilePictureID != new Guid("4fdd2f9f-bca8-4f90-8e27-ed432cbc39e0")) // TODO hard coded default imageID placeholder
                await _blobService.DeleteFileAsync(conversation.ProfilePictureID, "images", cancellationToken);
        }
        
        // if the ProfilePictureFileID is null then no new profile picture has been uploaded meaning we use the existing one
        var updatedConversation = Conversation.Update(conversation, dto.Name.Trim(), dto.ProfilePictureID ?? conversation.ProfilePictureID);
        if (updatedConversation.IsFailed)
            return Result.Fail<bool>(updatedConversation.Errors.Select(e => e.Message));
        
        await _conversations.UpdateAsync(updatedConversation.Value, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        //await _messageNotifier.NotifyConversationUpdatedAsync(updatedConversation.Value.ID, dto.Name, pfpResult, cancellationToken);
        //await _conversationNotifier.NotifyConversationUpdatedAsync(updatedConversation.Value.ID, dto.Name, pfpResult, cancellationToken);
        
        return Result.Ok(true);
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
        
        if(conversation.ProfilePictureID != new Guid("4fdd2f9f-bca8-4f90-8e27-ed432cbc39e0")) // TODO hard coded default imageID placeholder
            await _blobService.DeleteFileAsync(conversation.ProfilePictureID, "images", ct);
        
        await _conversations.DeleteAsync(conversation, ct);
        await _uow.SaveChangesAsync(ct);
        
        await _messageNotifier.NotifyConversationDeletedAsync(conversationID, ct);
        await _conversationNotifier.NotifyConversationDeletedAsync(conversationID, ct);
        
        return Result.Ok(true);
    }

    public async Task<Result<bool>> LeaveConversationAsync(Guid conversationID, Guid currentUserID, CancellationToken cancellationToken = default)
    {
        if (await _conversations.ExistsAsync(conversationID, cancellationToken) is false)
            return Result.Fail<bool>("Conversation does not exist.");
        
        var conversation = await _conversations.GetConversationAsync(conversationID, currentUserID, cancellationToken);
        if (conversation is null)
            return Result.Fail<bool>("Conversation not found or you are not part of it.");
        
        var participant = conversation.Participants.FirstOrDefault(p => p.UserID == currentUserID);
        if (participant is null)
            return Result.Fail<bool>("You are not a participant in this conversation.");
        
        await _conversations.DeleteParticipantAsync(participant, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(true);
    }

    public async Task<Result<bool>> RemoveParticipantAsync(Guid conversationID, Guid participantID, Guid currentUserID, CancellationToken ct = default)
    {
        if (await _conversations.ExistsAsync(conversationID, ct) is false)
            return Result.Fail<bool>("Conversation does not exist.");
        
        var conversation = await _conversations.GetConversationAsync(conversationID, currentUserID, ct);
        if (conversation is null)
            return Result.Fail<bool>("Conversation not found or you are not part of it.");
        
        if (conversation.CreatedByID != currentUserID)
            return Result.Fail<bool>("You can only remove participants from conversations you created.");
        
        var participant = conversation.Participants.FirstOrDefault(p => p.UserID == participantID);
        if (participant is null)
            return Result.Fail<bool>("Participant not found in this conversation.");
        
        await _conversations.DeleteParticipantAsync(participant, ct);
        await _uow.SaveChangesAsync(ct);
        
        await _messageNotifier.NotifyParticipantRemovedAsync(conversationID, participantID, ct);
        await _conversationNotifier.NotifyParticipantRemovedAsync(conversationID, participantID, ct);
        
        return Result.Ok(true);
    }
}