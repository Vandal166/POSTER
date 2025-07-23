using System.Text.Json;
using Application.Contracts;

namespace Infrastructure.Auth;

public class ROPTokenGeneratorService : ITokenGenerator<HttpResponseMessage>
{
    public static FormUrlEncodedContent CreateFormContent(Dictionary<string, string> parameters)
    {
        return new FormUrlEncodedContent(parameters);
    }
    
    // Generates a token using the Resource Owner Password Credentials (ROP) grant type.
    public async Task<HttpResponseMessage> GenerateTokenAsync(HttpClient client, FormUrlEncodedContent formUrlEncodedContent, CancellationToken cancellationToken = default)
    {
        //TODO use Authorization Code with PKCE instead of Resource Owner Password grant
        return await client.PostAsync(
            "protocol/openid-connect/token",
            formUrlEncodedContent,
            cancellationToken
        );
    }
   
    public async Task<HttpResponseMessage> SendRequestAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        // sending the request to the Keycloak server
        return await client.SendAsync(request, cancellationToken);
    }

    public static async Task<(string? idToken, string? accessToken, string? refreshToken)> ParseTokenResponseAsync(HttpResponseMessage res)    
    {
        var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;
        // some grants (ROP) do not return an ID token.
        string? idToken = json.TryGetProperty("id_token", out var idProp) ? idProp.GetString() : null;
        var accessToken = json.GetProperty("access_token").GetString() 
                          ?? throw new InvalidOperationException("Missing access_token");
        var refreshToken = json.GetProperty("refresh_token").GetString() 
                           ?? throw new InvalidOperationException("Missing refresh_token");
        return (idToken, accessToken, refreshToken);
    }
}