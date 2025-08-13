using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Account.Conversations;

[Authorize]
public class Details : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationRepository _conversationRepo;
    private readonly IConversationMessageRepository _conversationMessageRepo;
    
    public ConversationViewModel Conversation { get; private set; } = null!;
    public IEnumerable<MessageDto> Messages { get; private set; } = Enumerable.Empty<MessageDto>();
    
    public DateTime? FirstMessageAt { get; private set; }
    public const int PageSize = 6; // Default page size
    
    public Details(ICurrentUserService currentUser, IConversationRepository conversationRepo, 
        IConversationMessageRepository conversationMessageRepo)
    {
        _currentUser = currentUser;
        _conversationRepo = conversationRepo;
        _conversationMessageRepo = conversationMessageRepo;
    }
    
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct = default)
    {
        var conversation = await _conversationRepo.GetConversationViewModelAsync(id, _currentUser.ID, ct);
        if (conversation is null)
            return NotFound();
        
        Conversation = conversation;
        
        Messages = await _conversationMessageRepo
            .GetMessagesByConversationAsync(id, _currentUser.ID, null, PageSize, ct);

        if (Messages.Any())
        {
            FirstMessageAt = Messages.Last().CreatedAt;
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnGetPaged(Guid id, DateTime? firstMessageAt, CancellationToken ct = default)
    {
        var pagedMessages = await _conversationMessageRepo
            .GetMessagesByConversationAsync(id, _currentUser.ID, firstMessageAt, PageSize, ct);

        bool hasMore = pagedMessages.Count == PageSize;

        string nextUrl = hasMore
            ? $"?handler=Paged&id={id}&firstMessageAt={Uri.EscapeDataString(pagedMessages.Last().CreatedAt.ToString("o"))}"
            : string.Empty;
        
        
        var vm = new MessageLoaderViewModel
        {
            Messages = pagedMessages,
            HasMore = hasMore,
            NextUrl = nextUrl
        };

        return Partial("Shared/Account/Conversations/Messages/_MessageLoaderPartial", vm);
    }
    
    public async Task<IActionResult> OnGetNewMessagePartialAsync(Guid conversationId, Guid messageId, CancellationToken ct = default)
    {
        var message = await _conversationMessageRepo.GetMessageDtoAsync(conversationId, messageId, ct);
        if (message is null)
            return new EmptyResult();
     
        // We return the list partial, but with a model containing only the new message.
        return Partial("Shared/Account/Conversations/Messages/_MessageListPartial", new List<MessageDto> { message });
    }
}