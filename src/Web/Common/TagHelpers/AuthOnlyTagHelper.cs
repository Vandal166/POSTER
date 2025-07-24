using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;

namespace Web.Common.TagHelpers;

[HtmlTargetElement("auth-only")]
public class AuthOnlyTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthOnlyTagHelper(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var isAuthenticated = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
            output.SuppressOutput();
    }
}