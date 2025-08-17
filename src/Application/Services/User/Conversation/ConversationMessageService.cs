using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using FluentValidation;

namespace Application.Services;

internal sealed class ConversationMessageService : IConversationMessageService
{
    private readonly IConversationMessageRepository _conversationMessages;
    private readonly IValidator<CreateMessageDto> _createMessageValidator;
    private readonly IUnitOfWork _uow;
    
    
    public ConversationMessageService(IConversationMessageRepository conversationMessages, IValidator<CreateMessageDto> createMessageValidator, IUnitOfWork uow)
    {
        _conversationMessages = conversationMessages;
        _createMessageValidator = createMessageValidator;
        _uow = uow;
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
        
        return Result.Ok(message.Value.ID);
    }
    
    public async Task<Result<Guid>> CreateSystemMessageAsync(CreateMessageDto dto, CancellationToken ct = default)
    {
        var message = Message.CreateSystemMessage(dto.ConversationID, dto.Content);
        if (message.IsFailed)
            return Result.Fail<Guid>(message.Errors.Select(e => e.Message));
        
        await _conversationMessages.AddAsync(message.Value, ct);
        await _uow.SaveChangesAsync(ct);
        
        return Result.Ok(message.Value.ID);
    }

    public async Task<List<MessageDto>> GetMessagesByConversationAsync(Guid conversationID, Guid requestingUserID, DateTime? lastMessageAt, int pageSize, CancellationToken ct = default)
        => await _conversationMessages.GetMessagesByConversationAsync(conversationID, requestingUserID, lastMessageAt, pageSize, ct);

    public async Task<MessageDto?> GetMessageDtoAsync(Guid conversationID, Guid messageID, CancellationToken ct = default) 
        => await _conversationMessages.GetMessageDtoAsync(conversationID, messageID, ct);


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
        
        return Result.Ok(true);
    }
}