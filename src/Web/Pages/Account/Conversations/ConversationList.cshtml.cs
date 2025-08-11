using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.Services;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages.Account.Conversations;

[Authorize]
public class ConversationList : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationRepository _conversationRepo;
    private readonly IConversationService _conversationService;
    private readonly IUserRepository _userRepository;
    
    public IEnumerable<ConversationDto> Conversations { get; private set; } = Enumerable.Empty<ConversationDto>();
    public DateTime? LastMessageAt { get; private set; }
    public DateTime? LastConvCreationDate { get; private set; }
    
    public const int PageSize = 8; // Default page size
    
    [BindProperty]
    public CreateConversationDto ConversationDto { get; set; }
    
    public ConversationList(ICurrentUserService currentUser, IConversationRepository conversationRepo, 
        IConversationService conversationService, IUserRepository userRepository)
    {
        _currentUser = currentUser;
        _conversationRepo = conversationRepo;
        _conversationService = conversationService;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> OnGet(CancellationToken ct = default)
    {
        if(_currentUser.IsAuthenticated && _currentUser.HasClaim("profileCompleted", "false"))
            return RedirectToPage("/Account/CompleteProfile");

        await OnGetPaged(null, null, ct);
        return Page();
    }
    
    public async Task<IActionResult> OnGetPaged(DateTime? lastMessageAt,DateTime? lastConvCreationDate, CancellationToken ct = default)
    {
        var pagedConversations = await _conversationRepo.GetAllAsync(_currentUser.ID, lastMessageAt, lastConvCreationDate, PageSize, ct);
        bool hasMore = pagedConversations.Count == PageSize; // if the count is equal to PageSize, it means there are more conv available
        
        string nextUrl = string.Empty;

        if (hasMore && pagedConversations.Any())
        {
            var lastConv = pagedConversations.Last();
            var nextLastMessageAt = lastConv.LastMessageAt;
            var nextLastConvCreation = lastConv.CreatedAt;
        
            nextUrl = $"?handler=Paged" +
                      $"&lastMessageAt={Uri.EscapeDataString(nextLastMessageAt.ToString("o"))}" +
                      $"&lastConvCreationDate={Uri.EscapeDataString(nextLastConvCreation.ToString("o"))}";
        }
        
        var conversationDtos = pagedConversations.Select(p =>
            p with
            {
                Name = (p.ShouldTruncate(p.Name, 20) ? string.Concat(p.Name.AsSpan(0, 20), "...") : p.Name),
                LastMessageContent = (p.ShouldTruncate(p.LastMessageContent) ? string.Concat(p.LastMessageContent.AsSpan(0, 40), "...") : p.LastMessageContent)
            }).ToList();
        
        
        var vm = new ConversationLoaderViewModel
        {
            Conversations = conversationDtos,
            HasMore = hasMore,
            NextUrl = nextUrl
        };
        
        return Partial("Shared/Account/Conversations/_ConversationLoaderPartial", vm);
    }
    
    public async Task<IActionResult> OnGetNewConversationPartialAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await _conversationRepo.GetConversationDtoAsync(conversationId, _currentUser.ID, ct);
        if (conversation is null)
            return new EmptyResult();
        
        return Partial("Shared/Account/Conversations/_ConversationListPartial", new List<ConversationDto> { conversation });
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
        
        Response.Headers["HX-Redirect"] = Url.Page("/Account/Conversations/ConversationList");
        return new EmptyResult();
    }
}