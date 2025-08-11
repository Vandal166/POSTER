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
    
    public DeleteConversation(ICurrentUserService currentUser, IConversationService conversationService, IToastBuilder toastBuilder)
    {
        _currentUser = currentUser;
        _conversationService = conversationService;
        _toastBuilder = toastBuilder;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(Guid conversationId, CancellationToken ct = default)
    {
        var result = await _conversationService.DeleteConversationAsync(conversationId, _currentUser.ID, ct);

        _toastBuilder.SetToast(result)
            .OnSuccess("Conversation deleted successfully").Build(TempData);

        return RedirectToPage("/Account/Conversations/ConversationList");
    }
}