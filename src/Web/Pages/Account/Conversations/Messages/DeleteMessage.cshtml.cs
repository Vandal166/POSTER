using Application.Contracts;
using Application.Contracts.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages.Account.Conversations.Messages;

[Authorize]
public class DeleteMessage : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationMessageService _conversationMessageService;
    private readonly IToastBuilder _toastBuilder;
    private readonly IMessageNotifier _messageNotifier;
    
    public DeleteMessage(ICurrentUserService currentUser, IConversationMessageService conversationMessageService, IToastBuilder toastBuilder, IMessageNotifier messageNotifier)
    {
        _currentUser = currentUser;
        _conversationMessageService = conversationMessageService;
        _toastBuilder = toastBuilder;
        _messageNotifier = messageNotifier;
    }
    
    public IActionResult OnGet() => RedirectToPage("/Index");
    
    public async Task<IActionResult> OnPostAsync(Guid conversationId, Guid messageId, CancellationToken ct = default)
    {
        var result = await _conversationMessageService.DeleteMessageAsync(conversationId, messageId, _currentUser.ID, ct);

        _toastBuilder.SetToast(result)
            .OnSuccess("Message deleted successfully").Build(TempData);
        if (result.IsSuccess)
        {
            await _messageNotifier.NotifyMessageDeletedAsync(conversationId, messageId, ct);
        }
        
        return RedirectToPage("/Account/Conversations/Details", new { id = conversationId });
    }
}