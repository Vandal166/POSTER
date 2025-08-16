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

[Authorize, RedirectIncompleteUserProfile]
public class ConversationList : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationRepository _conversationRepo;
    private readonly IConversationService _conversationService;
    private readonly IUserRepository _userRepository;
    private readonly IConversationNotifier _conversationNotifier;
    private readonly INotificationRepository _notificationRepo;
    
    public IEnumerable<ConversationViewModel> Conversations { get; private set; } = Enumerable.Empty<ConversationViewModel>();
    public IEnumerable<Guid> ConversationsIds { get; private set; } = Enumerable.Empty<Guid>();
    
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public const int PageSize = 8; // Default page size
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
    
    [BindProperty]
    public CreateConversationDto ConversationDto { get; set; }
    
    public ConversationList(ICurrentUserService currentUser, IConversationRepository conversationRepo, 
        IConversationService conversationService, IUserRepository userRepository, IConversationNotifier conversationNotifier, INotificationRepository notificationRepo)
    {
        _currentUser = currentUser;
        _conversationRepo = conversationRepo;
        _conversationService = conversationService;
        _userRepository = userRepository;
        _conversationNotifier = conversationNotifier;
        _notificationRepo = notificationRepo;
    }

    public async Task<IActionResult> OnGet(int pageNumber = 1, CancellationToken ct = default)
    {
        var pagedConversations = await _conversationRepo.GetAllAsync(_currentUser.ID, pageNumber, PageSize, ct);
        ConversationsIds = await _conversationRepo.GetConversationsIdsAsync(_currentUser.ID, ct);
        Conversations = pagedConversations.Items.Select(dto => new ConversationViewModel
        {
            Conversation = dto with
            {
                Name = (dto.ShouldTruncate(dto.Name, 20) ? string.Concat(dto.Name.AsSpan(0, 20), "...") : dto.Name),
                LastMessageContent = (dto.ShouldTruncate(dto.LastMessageContent) ? string.Concat(dto.LastMessageContent.AsSpan(0, 40), "...") : dto.LastMessageContent)
            }
        }).ToList();
        
        CurrentPage = pagedConversations.Page;
        TotalPages = (int)Math.Ceiling(pagedConversations.TotalCount / (double)PageSize);
        
        return Page();
    }
    
    public async Task<IActionResult> OnGetNewConversationPartialAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await _conversationRepo.GetConversationDtoAsync(conversationId, _currentUser.ID, ct);
        if (conversation is null)
            return new EmptyResult();
        
        return Partial("Shared/Account/Conversations/_ConversationListPartial", new List<ConversationViewModel> { new ConversationViewModel { Conversation = conversation } });
    }
    
    public async Task<IActionResult> OnGetNewMessagePartialAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await _conversationRepo.GetConversationDtoAsync(conversationId, _currentUser.ID, ct);
        if (conversation is null)
            return new EmptyResult();
        
        return Partial("Shared/Account/Conversations/_ConversationListPartial", new List<ConversationViewModel> { new ConversationViewModel { Conversation = conversation } });
    }
    
    public async Task<PartialViewResult> OnGetUserSearchAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Partial("Shared/Account/Conversations/_UserSearchResultsPartial", Enumerable.Empty<UserDto>());
      
        var users = await _userRepository.SearchByUsernameAsync(username.Trim(), 1, 10, ct);
        return Partial("Shared/Account/Conversations/_UserSearchResultsPartial", users.Items);
    }
    
    public async Task<IActionResult> OnPostCreateAsync(string selectedUserIds, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return Partial("Shared/Account/Conversations/_CreateConversationFormPartial", ConversationDto);
        }
        
        var ids = selectedUserIds
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Guid.Parse)
            .Append(_currentUser.ID)
            .Distinct()
            .ToList();
        
        var result = await _conversationService.CreateConversationAsync(_currentUser.ID, ids, ConversationDto, ct);
        if (result.IsFailed)
        {
            return Partial("Shared/Account/Conversations/_CreateConversationFormPartial", ConversationDto)
                .WithHxToast(Response.HttpContext, $"Error: {result.Errors[0].Message}", "error");
        }
        
        var participants = ids!.Where(id => id != _currentUser.ID).ToList();
        await _conversationNotifier.NotifyConversationCreatedAsync(result.Value, participants, ct);
        await _notificationRepo.AddRangeAndSaveAsync(participants!.Select(id => Notification.Create(id, $"{_currentUser.Username} has added you to a conversation",
                $"/Account/Conversations/Details/{result.Value}").Value), ct);
        
        Response.Headers["HX-Redirect"] = Url.Page("/Account/Conversations/ConversationList");
        return new EmptyResult();
    }
}