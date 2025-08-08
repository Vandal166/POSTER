using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.Services;
using Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Common;

namespace Web.Pages.Messages;

[Authorize]
public class ConversationList : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConversationRepository _conversationRepo;
    private readonly IConversationService _conversationService;
    private readonly IUserRepository _userRepository;
    
    public IEnumerable<ConversationDto> Conversations { get; private set; } = Enumerable.Empty<ConversationDto>();
    public DateTime? LastMessageAt { get; private set; }
    
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

        await OnGetPaged(null, ct);
        return Page();
    }
    
    public async Task<IActionResult> OnGetPaged(DateTime? lastMessageAt, CancellationToken ct = default)
    {
        var pagedConversations = await _conversationRepo.GetAllAsync(_currentUser.ID, lastMessageAt, PageSize, ct);
        bool hasMore = pagedConversations.Count == PageSize; // if the count is equal to PageSize, it means there are more conv available
        
        string nextUrl = hasMore
            ? $"?handler=Paged&lastMessageAt={Uri.EscapeDataString(pagedConversations.Last().LastMessageAt.ToString("o"))}"
            : string.Empty;
        
        var conversationDtos = pagedConversations.Select(p =>
            p with
            {
                Name = (p.ShouldTruncate(p.Name, 20) ? string.Concat(p.Name.AsSpan(0, 20), "...") : p.Name),
                Content = (p.ShouldTruncate(p.Content) ? string.Concat(p.Content.AsSpan(0, 40), "...") : p.Content)
            }).ToList();
        
        
        var vm = new ConversationLoaderViewModel
        {
            Conversations = conversationDtos,
            HasMore = hasMore,
            NextUrl = nextUrl
        };
        
        return Partial("Shared/Account/Conversations/_ConversationLoaderPartial", vm);
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
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
            return Partial("Shared/Account/Conversations/_CreateConversationFormPartial", ConversationDto)
                .WithHxToast(Response.HttpContext, "Error creating conversation", "error");
        }
        
        Response.Headers["HX-Redirect"] = Url.Page("/Account/Conversations/ConversationList");
        return new EmptyResult();
    }
}