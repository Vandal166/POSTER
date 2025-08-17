using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
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
    private readonly INotificationRepository _notificationRepo;
    
    public RemoveParticipant(ICurrentUserService currentUser, IConversationService conversationService, 
        IToastBuilder toastBuilder, IConversationMessageService conversationMessageService, 
        IMessageNotifier messageNotifier, IConversationNotifier conversationNotifier, INotificationRepository notificationRepo)
    {
        _currentUser = currentUser;
        _conversationService = conversationService;
        _toastBuilder = toastBuilder;
        _conversationMessageService = conversationMessageService;
        _messageNotifier = messageNotifier;
        _conversationNotifier = conversationNotifier;
        _notificationRepo = notificationRepo;
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
            
            await _notificationRepo.AddAndSaveAsync(Notification.Create(participantId, $"{_currentUser.Username} has removed you from the conversation").Value, ct);
            
            await _conversationMessageService.CreateSystemMessageAsync
                (new CreateMessageDto(conversationId, $"{participantName} has been removed from the conversation."), ct);
        }//TODO the admin can set the name of the participant via dev tools to smth like: <script>alert(1)</script>
        
        return RedirectToPage("/Account/Conversations/Details", new { id = conversationId });
    }
}