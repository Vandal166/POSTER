using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using FluentValidation;

namespace Application.Services;

public sealed class ConversationMessageService : IConversationMessageService
{
    private readonly IConversationMessageRepository _conversationMessages;
    private readonly IValidator<CreateMessageDto> _createMessageValidator;
    private readonly IUnitOfWork _uow;
    private readonly IMessageNotifier _messageNotifier;
    
    public ConversationMessageService(IConversationMessageRepository conversationMessages, IValidator<CreateMessageDto> createMessageValidator, IUnitOfWork uow, IMessageNotifier messageNotifier)
    {
        _conversationMessages = conversationMessages;
        _createMessageValidator = createMessageValidator;
        _uow = uow;
        _messageNotifier = messageNotifier;
    }


    public async Task<Result<Guid>> CreateMessageAsync(Guid currentUserID, CreateMessageDto dto, CancellationToken ct = default)
    {
        var validation = await _createMessageValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail<Guid>(validation.Errors.Select(e => e.ErrorMessage));
        
        var message = Message.Create(currentUserID, dto.ConversationID, dto.Content, dto.VideoFileID);
        if (message.IsFailed)
            return Result.Fail<Guid>(message.Errors.Select(e => e.Message));
        
        await _conversationMessages.AddAsync(message.Value, ct);
        await _uow.SaveChangesAsync(ct);

        await _messageNotifier.NotifyMessageCreatedAsync(message.Value.ConversationID, message.Value.ID, ct);
        
        return Result.Ok(message.Value.ID);
    }

    public async Task<IPagedList<MessageDto>> GetMessagesByConversationAsync(Guid conversationID, Guid currentUserID, DateTime? firstMessageAt, int pageSize,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<MessageDto?> GetMessageAsync(Guid messageID, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> DeleteMessageAsync(Guid conversationID, Guid messageID, Guid currentUserID, CancellationToken ct = default)
    {
        if(await _conversationMessages.ExistsAsync(conversationID, messageID, ct) is false)
            return Result.Fail<bool>("Message does not exist in the conversation.");
        
        var message = await _conversationMessages.GetMessageAsync(conversationID, messageID, ct);
        if (message is null)
            return Result.Fail<bool>("Message not found.");
        
        if (message.SenderID != currentUserID)
            return Result.Fail<bool>("You can only delete your own messages.");
        
        await _conversationMessages.DeleteAsync(message, ct);
        await _uow.SaveChangesAsync(ct);
        
        await _messageNotifier.NotifyMessageDeletedAsync(conversationID, messageID, ct);
        
        return Result.Ok(true);
    }
}