using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Contracts;
using Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public interface IKeycloakService
{
    Task<KeycloakUser?> GetUserAsync(string userID, CancellationToken ct = default);
}

public class KeycloakService : IKeycloakService
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public KeycloakService(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<KeycloakUser?> GetUserAsync(string userID, CancellationToken ct = default)
    {
        var keyc = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");

        var tokenRes = await client.PostAsync(
            "protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = keyc["AdminClientId"],
                ["client_secret"] = keyc["AdminClientSecret"]
            }),
            ct
        );

        if (!tokenRes.IsSuccessStatusCode) return null;

        var adminToken = JsonDocument.Parse(await tokenRes.Content.ReadAsStringAsync(ct))
            .RootElement.GetProperty("access_token").GetString();

        // Fetch the latest user
        var req = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/{keyc["Realm"]}/users/{userID}"
        );
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return null;

        var json = await res.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<KeycloakUser>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}

public class KeycloakUser
{
    public string ID { get; set; }
    public bool Enabled { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public Dictionary<string, string[]> Attributes { get; set; }
    public List<string> RealmRoles { get; set; } = new();
}
/* EXAMPLE OUTPUT OF JSON DESERIALIZATION
 * {"id":"2af5dc02-faea-42f3-9245-44384d965b92","username":"testuser","email":"testuser@onet.pl",
 * "emailVerified":false,"attributes":{"profileCompleted":["true"]},"enabled":true,
 * "createdTimestamp":1753274517777,"totp":false,"disableableCredentialTypes":[],"requiredActions":[],"notBefore":0,
 * "access":{"manageGroupMembership":true,"view":true,
 * "mapRoles":true,"impersonate":false,"manage":true}}
 */



public static class KeycloakClientFactory
{
    public static object CreateKeycloakUser(RegisterUserDto dto)
    {
        return new 
        {
            username = dto.Email,
            email    = dto.Email,
            enabled  = true,
            attributes = new {
                profileCompleted = new[] { "false" }
            },
            credentials = new[] {
                new { type = "password", value = dto.Password, temporary = false }
            }
        };
    }
    
    public static async Task<HttpResponseMessage?> GetKeycloakUser(string userID, HttpClient client, ITokenGenerator<HttpResponseMessage> _ropTokenGenerator,
        string? adminToken, IConfigurationSection configSection, CancellationToken ct)
    {
        // getting the existing user
        var getReq = new HttpRequestMessage
        (
            HttpMethod.Get,
            $"/admin/realms/{configSection["Realm"]}/users/{userID}"
        );
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        return await _ropTokenGenerator.SendRequestAsync(client, getReq, ct);
    }
    
    //TODO create an class 'Keycloakuser' to represent the user object
    public static async Task<HttpResponseMessage> SetKeycloakUsername(string userID, string userName, string userJson, HttpClient client, ITokenGenerator<HttpResponseMessage> ropTokenGenerator,
        string? adminToken, IConfigurationSection configSection, CancellationToken ct)
    {
        // 3) Build a mutable dictionary from the JSON
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(userJson)!;

        // 4) Apply your updates
        dict["username"] = userName;
        dict["attributes"] = new Dictionary<string, string[]>
        {
            ["profileCompleted"] = new[] { "true" }
        };

        // 5) PUT the full object back
        var putReq = new HttpRequestMessage
        (
            HttpMethod.Put,
            $"/admin/realms/{configSection["Realm"]}/users/{userID}"
        )
        {
            Content = new StringContent
            (
                JsonSerializer.Serialize(dict),
                Encoding.UTF8,
                "application/json"
            )
        };
        putReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        return await ropTokenGenerator.SendRequestAsync(client, putReq, ct);
    }
}