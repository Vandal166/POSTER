using System.Text.Json;
using Application.Contracts.Auth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public class SessionValidator : ISessionValidator
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public SessionValidator(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }
    
    public async Task<bool> IsTokenActiveAsync(string token, CancellationToken ct = default)
    {
        var keycloak = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");

        var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = keycloak["ClientId"],
            ["client_secret"] = keycloak["ClientSecret"],
            ["token"] = token
        });

        var response = await client.PostAsync(
            $"/realms/{keycloak["Realm"]}/protocol/openid-connect/token/introspect", 
            formContent, 
            ct
        );

        if (!response.IsSuccessStatusCode) 
            return false;

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return json.RootElement.GetProperty("active").GetBoolean();
    }
}