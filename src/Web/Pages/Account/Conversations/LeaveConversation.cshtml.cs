using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages.Account.Conversations;

[Authorize]
public class LeaveConversation : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationService _conversationService;
    private readonly IConversationMessageService _conversationMessageService;
    private readonly IToastBuilder _toastBuilder;
    
    public LeaveConversation(ICurrentUserService currentUser, IConversationService conversationService,
        IConversationMessageService conversationMessageService, IToastBuilder toastBuilder)
    {
        _currentUser = currentUser;
        _conversationService = conversationService;
        _conversationMessageService = conversationMessageService;
        _toastBuilder = toastBuilder;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(Guid conversationId, CancellationToken ct = default)
    {
        var result = await _conversationService.LeaveConversationAsync(conversationId, _currentUser.ID, ct);

        _toastBuilder.SetToast(result)
            .OnSuccess("Successfully left the conversation").Build(TempData);

        if (result.IsSuccess)
        {
            await _conversationMessageService.CreateSystemMessageAsync
                (new CreateMessageDto(conversationId, $"{_currentUser.Username} has left the conversation."), ct);
        }

        
        return RedirectToPage("/Account/Conversations/ConversationList");
    }
}