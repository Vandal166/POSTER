using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Contracts;

namespace Web.Pages;

[Authorize]
public class Profile : PageModel
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;
    private readonly IToastBuilder _toastBuilder;
    
    public string AvatarPath { get; set; }
    
    public Profile(IUserService userService, ICurrentUserService currentUser, IToastBuilder toastBuilder)
    {
        _userService = userService;
        _currentUser = currentUser;
        _toastBuilder = toastBuilder;
    }
    
    public PageResult OnGet()
    {
        AvatarPath = _currentUser.AvatarPath;
        return Page();
    }
    
    public async Task<IActionResult> OnPostAvatarAsync(IFormFile? avatar, CancellationToken ct)
    {
        if (avatar == null || avatar.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a valid avatar file.");
            return Page();
        }
    
        var dto = new AvatarDto(avatar.FileName, avatar.OpenReadStream());
    
        var result = await _userService.UpdateAvatarAsync(_currentUser.ID, dto, ct);
    
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors.Select(e => e.Message)));
            return Page();
        }
        
        _toastBuilder.SetToast("Avatar updated successfully", ToastType.Success)
            .Build(TempData);
        
        return RedirectToPage("/Account/Profile");
    }
}