using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web.Common.TagHelpers;

[HtmlTargetElement("anon-only")]
public class AnonOnlyTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AnonOnlyTagHelper(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var isAuthenticated = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        if (isAuthenticated)
            output.SuppressOutput();
    }
}