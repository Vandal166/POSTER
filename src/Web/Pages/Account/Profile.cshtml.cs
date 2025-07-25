using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

[Authorize]
public class Profile : PageModel
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;
    public Profile(IUserService userService, ICurrentUserService currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }
    
    [BindProperty]
    public UsernameDto UsernameDto { get; set; }
    
    [BindProperty]
    public AvatarDto AvatarDto { get; set; }
    
    [BindProperty]
    public IFormFile AvatarFile { get; set; }
    public string AvatarPath { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        AvatarPath = await _userService.GetAvatarPathAsync(_currentUser.ID);
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAvatarAsync(IFormFile? avatar, CancellationToken ct)
    {
        if (avatar == null || avatar.Length == 0)
            return BadRequest("No file uploaded.");
    
        var dto = new AvatarDto(
            avatar.FileName,
            avatar.OpenReadStream()
        );
    
        var result = await _userService.UpdateAvatarAsync(_currentUser.ID, dto, ct);
    
        if (result.IsFailed)
            return BadRequest(result.Errors);
    
        return RedirectToPage("/Account/Profile");
    }
}