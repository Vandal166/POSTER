using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web.Common.TagHelpers;

[HtmlTargetElement("role-only")]
public class RoleOnlyTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public RoleOnlyTagHelper(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string Roles { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            output.SuppressOutput();
            return;
        }

        var allowedRoles = Roles.Split(',').Select(r => r.Trim());
        if (!allowedRoles.Any(user.IsInRole))
            output.SuppressOutput();
    }
}