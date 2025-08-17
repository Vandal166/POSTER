using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages.Account.Conversations;

[Authorize]
public class Details : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationRepository _conversationRepo;
    private readonly IConversationService _conversationService;
    private readonly IConversationMessageService _conversationMessageService;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepo;
    
    public ConversationViewModel Conversation { get; private set; } = null!;
    public IEnumerable<MessageDto> Messages { get; private set; } = Enumerable.Empty<MessageDto>();
    
    public DateTime? FirstMessageAt { get; private set; }
    public const int PageSize = 6; // Default page size
    
    public Details(ICurrentUserService currentUser, IConversationRepository conversationRepo,
        IConversationService conversationService, IConversationMessageService conversationMessageService,
        IUserRepository userRepository, INotificationRepository notificationRepo)
    {
        _currentUser = currentUser;
        _conversationRepo = conversationRepo;
        _conversationService = conversationService;
        _conversationMessageService = conversationMessageService;
        _userRepository = userRepository;
        _notificationRepo = notificationRepo;
    }
    
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct = default)
    {
        var conversation = await _conversationRepo.GetConversationViewModelAsync(id, _currentUser.ID, ct);
        if (conversation is null)
            return NotFound();
        
        Conversation = conversation;
        
        Messages = await _conversationMessageService
            .GetMessagesByConversationAsync(id, _currentUser.ID, null, PageSize, ct);

        if (Messages.Any())
        {
            FirstMessageAt = Messages.Last().CreatedAt;
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnGetPaged(Guid id, DateTime? firstMessageAt, CancellationToken ct = default)
    {
        var pagedMessages = await _conversationMessageService
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
        var message = await _conversationMessageService.GetMessageDtoAsync(conversationId, messageId, ct);
        if (message is null)
            return new EmptyResult();
     
        // We return the list partial, but with a model containing only the new message.
        return Partial("Shared/Account/Conversations/Messages/_MessageListPartial", new List<MessageDto> { message });
    }
    
    public async Task<PartialViewResult> OnGetUserSearchAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Partial("Shared/Account/Conversations/_UserSearchResultsPartial", Enumerable.Empty<UserDto>());
      
        var users = await _userRepository.SearchByUsernameAsync(username.Trim(), 1, 10, ct);
        return Partial("Shared/Account/Conversations/_UserSearchResultsPartial", users.Items);
    }
    
    public async Task<IActionResult> OnPostAddUsersAsync(Guid conversationId, string selectedUserIds, CancellationToken ct = default)
    {
        var conversation = await _conversationRepo.GetConversationViewModelAsync(conversationId, _currentUser.ID, ct);
        if (conversation is null)
            return NotFound();
        
        Conversation = conversation;
        
        if (!ModelState.IsValid)
        {
            return Partial("Shared/Account/Conversations/_AddUserToConversationFormPartial", Conversation.Conversation);
        }
        
        var ids = selectedUserIds
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Guid.Parse)
            .Distinct()
            .ToList();
        
        var result = await _conversationService.AddParticipantAsync(Conversation.Conversation.Id, ids, _currentUser.ID, ct);
        if (result.IsFailed)
        {
            return Partial("Shared/Account/Conversations/_AddUserToConversationFormPartial", Conversation.Conversation)
                .WithHxToast(Response.HttpContext, $"Error: {result.Errors[0].Message}", "error");
        }
        
        foreach (var participantId in ids!.Where(id => id != _currentUser.ID))
        {
            var participant = await _userRepository.GetUserDtoAsync(participantId, ct);
            if (participant is null)
                continue;
            
            await _notificationRepo.AddAndSaveAsync(Notification.Create(participant.Id, $"{_currentUser.Username} has added you to the {conversation.Conversation.Name} conversation",
                $"/Account/Conversations/Details/{Conversation.Conversation.Id}").Value, ct);
            
            await _conversationMessageService.CreateSystemMessageAsync(
                new CreateMessageDto(Conversation.Conversation.Id, $"{participant.Username} has been added to the conversation."), ct);
        }
        
        Response.Headers["HX-Redirect"] = Url.Page("/Account/Conversations/Details", new { id = Conversation.Conversation.Id });
        return new EmptyResult();
    }
}