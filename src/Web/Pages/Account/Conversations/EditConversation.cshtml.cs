using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;
using Web.Contracts;

namespace Web.Pages.Account.Conversations;

[Authorize]
public class EditConversation : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationRepository _conversationRepo;
    private readonly IConversationService _conversationService;
    private readonly IConversationMessageService _conversationMessageService;

    public EditConversation(ICurrentUserService currentUser, IConversationRepository conversationRepo, 
        IConversationService conversationService, IConversationMessageService conversationMessageService, 
        IToastBuilder toastBuilder)
    {
        _currentUser = currentUser;
        _conversationRepo = conversationRepo;
        _conversationService = conversationService;
        _conversationMessageService = conversationMessageService;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostEditAsync([FromForm] UpdateConversationDto conversation, CancellationToken ct = default)
    {
        var c = await _conversationRepo.GetConversationViewModelAsync(conversation.Id, _currentUser.ID, ct);
        if (c is null)
            return NotFound();
        
        var model = new ConversationViewModel
        {
            Conversation = c.Conversation with
            {
                Name = conversation.Name, 
                ProfilePictureID = conversation.ProfilePictureID ?? c.Conversation.ProfilePictureID
            },
            Participants = c.Participants
        };
        
        if (!ModelState.IsValid)
        {
            return Partial("Shared/Account/Conversations/_EditConversationFormPartial", model);
        }
        
        var result = await _conversationService.UpdateConversationAsync(conversation, _currentUser.ID, ct);
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
            return Partial("Shared/Account/Conversations/_EditConversationFormPartial", model).WithHxToast(Response.HttpContext, $"Error: {result.Errors[0].Message}", "error");
        }
        
        await _conversationMessageService.CreateSystemMessageAsync(
            new CreateMessageDto(c.Conversation.Id, $"{_currentUser.Username} has updated the conversation."), ct);
       
        Response.Headers["HX-Redirect"] = Url.Page("/Account/Conversations/Details", new { id = c.Conversation.Id });
        return new EmptyResult();
    }
}