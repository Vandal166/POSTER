using System.Text.Json;
using Application.Contracts.Auth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

internal sealed class AdminTokenProvider : IAdminTokenProvider
{
    private readonly IHttpClientFactory _http;
    private readonly ITokenGenerator<HttpResponseMessage> _tokenGen;
    private readonly IConfiguration _config;

    public AdminTokenProvider(IHttpClientFactory http,
        ITokenGenerator<HttpResponseMessage> tokenGen,
        IConfiguration config)
    {
        _http = http;
        _tokenGen = tokenGen;
        _config = config;
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        var keyc   = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");
        var form = ROPTokenGeneratorService.CreateFormContent
        (
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = keyc["AdminClientId"],
                ["client_secret"] = keyc["AdminClientSecret"]
            }
        );

        var res = await _tokenGen.GenerateTokenAsync(client, form, ct);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException("Cannot acquire admin token.");
        
        var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        var accessToken = json.RootElement.GetProperty("access_token").GetString();
        return accessToken!;
    }
}