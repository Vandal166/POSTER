using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages;

[Authorize(Policy = "ProfileNotCompleted")] // only allow authenticated users who haven't completed their profile
public class CompleteProfile : PageModel
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserService _userService;
    private readonly IToastBuilder _toastBuilder;

    public CompleteProfile(ICurrentUserService currentUser, IUserService userService, IToastBuilder toastBuilder)
    {
        _currentUser = currentUser;
        _userService = userService;
        _toastBuilder = toastBuilder;
    }

    [BindProperty]
    public UsernameDto Dto { get; set; }

    public IActionResult OnGet()
    {
        _toastBuilder.SetToast("Complete your profile to continue", ToastType.Error) //TODO create Warning type
            .Build(TempData);
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _userService.UpdateUsernameAsync(_currentUser.ID, Dto, ct);
        if (result.IsFailed)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Message);
            return Page();
        }
        
        _toastBuilder.SetToast("Profile updated successfully", ToastType.Success)
            .Build(TempData);
        
        return RedirectToPage("/Index");
    }
}