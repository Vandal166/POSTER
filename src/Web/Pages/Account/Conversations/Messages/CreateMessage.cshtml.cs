using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Account.Conversations.Messages;

[Authorize]
public class CreateMessage : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationMessageService _conversationMessageService;
    private readonly IMessageNotifier _messageNotifier;
    private readonly IConversationNotifier _conversationNotifier;
    
    [BindProperty]
    public CreateMessageDto MessageDto { get; set; }
    
    public CreateMessage(ICurrentUserService currentUser, IConversationMessageService conversationMessageService, IMessageNotifier messageNotifier, IConversationNotifier conversationNotifier)
    {
        _currentUser = currentUser;
        _conversationMessageService = conversationMessageService;
        _messageNotifier = messageNotifier;
        _conversationNotifier = conversationNotifier;
    }
    
    
    public async Task<IActionResult> OnPostMessageOnConversationAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = new CreateMessageViewModel
            {
                ConversationId = MessageDto.ConversationID,
                Content = MessageDto.Content,
            };
            return Partial("Shared/Account/Conversations/Messages/_CreateMessageFormPartial", viewModel);
        }

        var result = await _conversationMessageService.CreateMessageAsync(_currentUser.ID, MessageDto, ct);

        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
            var viewModel = new CreateMessageViewModel
            {
                ConversationId = MessageDto.ConversationID,
                Content = MessageDto.Content,
            };
            return Partial("Shared/Account/Conversations/Messages/_CreateMessageFormPartial", viewModel);
        }

        await _messageNotifier.NotifyMessageCreatedAsync(MessageDto.ConversationID, result.Value, ct);
        await _conversationNotifier.NotifyMessageCreated(MessageDto.ConversationID, ct);

        Response.Headers["HX-Redirect"] = Url.Page("/Account/Conversations/Details", new { id = MessageDto.ConversationID });
        return new EmptyResult();
    }
}