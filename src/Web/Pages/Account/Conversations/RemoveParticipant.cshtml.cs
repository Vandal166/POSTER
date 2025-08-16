using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages.Account.Conversations;

[Authorize]
public class RemoveParticipant : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationService _conversationService;
    private readonly IToastBuilder _toastBuilder;
    private readonly IConversationMessageService _conversationMessageService;
    private readonly IMessageNotifier _messageNotifier;
    private readonly IConversationNotifier _conversationNotifier;
    
    public RemoveParticipant(ICurrentUserService currentUser, IConversationService conversationService, 
        IToastBuilder toastBuilder, IConversationMessageService conversationMessageService, 
        IMessageNotifier messageNotifier, IConversationNotifier conversationNotifier)
    {
        _currentUser = currentUser;
        _conversationService = conversationService;
        _toastBuilder = toastBuilder;
        _conversationMessageService = conversationMessageService;
        _messageNotifier = messageNotifier;
        _conversationNotifier = conversationNotifier;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(Guid conversationId, Guid participantId, string participantName, CancellationToken ct = default)
    {
        var result = await _conversationService.RemoveParticipantAsync(conversationId, participantId, _currentUser.ID, ct);
        
        _toastBuilder.SetToast(result)
            .OnSuccess("Participant removed successfully").Build(TempData);
          
        if (result.IsSuccess)
        {
            await _messageNotifier.NotifyParticipantRemovedAsync(conversationId, participantId, ct);
            await _conversationNotifier.NotifyParticipantRemovedAsync(conversationId, participantId, ct);
            
            await _conversationMessageService.CreateSystemMessageAsync
                (new CreateMessageDto(conversationId, $"{participantName} has been removed from the conversation."), ct);
        }//TODO the admin can set the name of the participant via dev tools to smth like: <script>alert(1)</script>
        
        return RedirectToPage("/Account/Conversations/Details", new { id = conversationId });
    }
}