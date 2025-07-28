using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Application.Contracts;
using Application.Contracts.Auth;
using Application.DTOs;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public class PasswordTokenProvider : IPasswordTokenProvider
{
    private readonly IHttpClientFactory _http;
    private readonly ITokenGenerator<HttpResponseMessage> _tokenGen;
    private readonly IConfiguration _config;

    public PasswordTokenProvider(
        IHttpClientFactory http,
        ITokenGenerator<HttpResponseMessage> tokenGen,
        IConfiguration config)
    {
        _http = http;
        _tokenGen = tokenGen;
        _config = config;
    }

    public async Task<IResult<TokenResponse>> ExchangeAsync(string login, string password, CancellationToken ct = default)
    {
        var keyc   = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");

        var form = ROPTokenGeneratorService.CreateFormContent(new Dictionary<string,string>{
            ["grant_type"]    = "password",
            ["client_id"]     = keyc["ClientId"]!,
            ["client_secret"] = keyc["ClientSecret"]!,
            ["username"]      = login,
            ["password"]      = password
        });

        var res = await _tokenGen.GenerateTokenAsync(client, form, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            return Result.Fail<TokenResponse>($"Login failed: {GetFriendlyMessage(body)}");

        var (idToken, accessToken, refreshToken) = await ROPTokenGeneratorService.ParseTokenResponseAsync(res);

        // parse the "sub" from the access token:
        var jwt   = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var userID   = jwt.Claims.FirstOrDefault(c=>c.Type=="sub")?.Value;
        if (Guid.TryParse(userID, out var guid) is false)
            return Result.Fail<TokenResponse>("Unable to load user profile.");

        return Result.Ok(new TokenResponse(accessToken, idToken, refreshToken, guid));
    }
    
    private static string GetFriendlyMessage(string body)
    {
        return JsonDocument.Parse(body).RootElement.GetProperty("error_description").GetString() ?? "An error occurred.";
    }
}