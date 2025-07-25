using Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class IndexModel : PageModel
{
    private readonly ICurrentUserService _currentUser;
    public IndexModel(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public string? UserID => _currentUser.ID;
    
    public IActionResult OnGet()
    {
        var user = HttpContext.User;
        
        if(user?.Identity?.IsAuthenticated != false && _currentUser.HasClaim("profileCompleted", "false"))
            return RedirectToPage("/Account/CompleteProfile");
        return Page();
    }
}