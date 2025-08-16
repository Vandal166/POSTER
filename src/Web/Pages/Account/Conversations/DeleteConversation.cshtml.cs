using Application.Contracts;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages.Account.Conversations;

[Authorize]
public class DeleteConversation : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationService _conversationService;
    private readonly IToastBuilder _toastBuilder;
    private readonly IMessageNotifier _messageNotifier;
    private readonly IConversationNotifier _conversationNotifier;
    
    public DeleteConversation(ICurrentUserService currentUser, IConversationService conversationService, 
        IToastBuilder toastBuilder, IMessageNotifier messageNotifier, IConversationNotifier conversationNotifier)
    {
        _currentUser = currentUser; 
        _conversationService = conversationService;
        _toastBuilder = toastBuilder;
        _messageNotifier = messageNotifier;
        _conversationNotifier = conversationNotifier;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(Guid conversationId, CancellationToken ct = default)
    {
        var result = await _conversationService.DeleteConversationAsync(conversationId, _currentUser.ID, ct);

        _toastBuilder.SetToast(result)
            .OnSuccess("Conversation deleted successfully").Build(TempData);
        if (result.IsSuccess)
        {
            await _messageNotifier.NotifyConversationDeletedAsync(conversationId, ct);
            await _conversationNotifier.NotifyConversationDeletedAsync(conversationId, ct);
        }


        return RedirectToPage("/Account/Conversations/ConversationList");
    }
}