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
    
    public DeleteMessage(ICurrentUserService currentUser, IConversationMessageService conversationMessageService, IToastBuilder toastBuilder)
    {
        _currentUser = currentUser;
        _conversationMessageService = conversationMessageService;
        _toastBuilder = toastBuilder;
    }
    
    public IActionResult OnGet() => RedirectToPage("/Index");
    
    public async Task<IActionResult> OnPostAsync(Guid conversationId, Guid messageId, CancellationToken ct = default)
    {
        var result = await _conversationMessageService.DeleteMessageAsync(conversationId, messageId, _currentUser.ID, ct);
        
        _toastBuilder.SetToast(result)
            .OnSuccess("Message deleted successfully").Build(TempData);

        return RedirectToPage("/Account/Conversations/Details", new { id = conversationId });
    }
}