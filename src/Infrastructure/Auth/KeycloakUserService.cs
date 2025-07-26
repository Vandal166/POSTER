using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Contracts;
using Application.Contracts.Auth;
using Application.DTOs;
using Domain.Constants;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public class KeycloakUserService : IKeycloakUserService
{
    private readonly IHttpClientFactory _http;
    private readonly ICurrentUserService _currentUser;
    private readonly ITokenGenerator<HttpResponseMessage> _tokenGen;
    private readonly IConfiguration _config;
    private IConfigurationSection _keyConf => _config.GetSection("Keycloak");
    
    public KeycloakUserService(IHttpClientFactory http, ICurrentUserService currentUser, ITokenGenerator<HttpResponseMessage> tokenGen,
        IConfiguration config)
    {
        _http = http;
        _currentUser = currentUser;
        _tokenGen = tokenGen;
        _config = config;
    }

    public async Task<IResult<string>> CreateAsync(RegisterUserDto dto, string adminToken, CancellationToken ct = default)
    {
        var client = _http.CreateClient("Keycloak");

        var userPayload = KeycloakClientFactory.CreateKeycloakUser(dto);
        var req = new HttpRequestMessage(HttpMethod.Post,
            $"/admin/realms/{_keyConf["Realm"]}/users")
        {
            Content = new StringContent(JsonSerializer.Serialize(userPayload),
                Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var res = await _tokenGen.SendRequestAsync(client, req, ct); // this creates the user(if no errors) in keycloak
        if (res.StatusCode == HttpStatusCode.Conflict)
        {
            return Result.Fail<string>("User already exists.");
        }

        if (res.StatusCode != HttpStatusCode.Created)
        {
            return Result.Fail<string>($"Error: {res.StatusCode}");
        }

        // Location header ends with the new user ID
        var locationSegments = res.Headers.Location?.Segments;
        var newId = locationSegments?.Last();
        if (string.IsNullOrEmpty(newId))
        {
            return Result.Fail<string>("Failed to retrieve new user ID.");
        }

        return Result.Ok(newId);
    }

    public async Task<IResult<string>> GetAsync(string userID, string adminToken, CancellationToken ct = default)
    {
        var client = _http.CreateClient("Keycloak");

        // getting the existing user
        var getReq = new HttpRequestMessage
        (
            HttpMethod.Get,
            $"/admin/realms/{_keyConf["Realm"]}/users/{userID}"
        );
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var getRes =  await _tokenGen.SendRequestAsync(client, getReq, ct);
        
        if (getRes is null || !getRes.IsSuccessStatusCode)
            return Result.Fail<string>($"Cannot fetch user: {getRes?.StatusCode}");

        var userJson = await getRes.Content.ReadAsStringAsync(ct);
        return Result.Ok(userJson);
    }
    
    public async Task<KeycloakUser?> GetUserAsync(string userID, CancellationToken ct = default)
    {
        var client = _http.CreateClient("Keycloak");

        var tokenRes = await client.PostAsync(
            "protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _keyConf["AdminClientId"],
                ["client_secret"] = _keyConf["AdminClientSecret"]
            }),
            ct
        );

        if (!tokenRes.IsSuccessStatusCode) 
            return null;

        var adminToken = JsonDocument.Parse(await tokenRes.Content.ReadAsStringAsync(ct))
            .RootElement.GetProperty("access_token").GetString();

        // Fetch the latest user
        var req = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/{_keyConf["Realm"]}/users/{userID}"
        );
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) 
            return null;

        var json = await res.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<KeycloakUser>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private async Task<IResult<string>> UpdateUserAsync(string userID, string userJson, string adminToken, Action<Dictionary<string, object>> updateAction,
        string conflictMessage = "Conflict.", CancellationToken ct = default)
    {
        var client = _http.CreateClient("Keycloak");
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(userJson)!;

        updateAction(dict); // triggering the action for updating, currently <updateUsername, updateAvatar>

        var putReq = new HttpRequestMessage(
            HttpMethod.Put,
            $"/admin/realms/{_keyConf["Realm"]}/users/{userID}")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(dict),
                Encoding.UTF8,
                "application/json")
        };
        putReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var putRes = await _tokenGen.SendRequestAsync(client, putReq, ct);

        if (putRes.IsSuccessStatusCode)
        {
            var user = await GetUserAsync(userID, ct);
            if (user is null)
                return Result.Fail<string>("Unable to fetch user profile after update.");
            
            var newPrincipal = ClaimsPrincipalFactory.BuildClaims(user);
            await _currentUser.RefreshClaims(newPrincipal);
            
            return Result.Ok(userID);
        }

        return putRes.StatusCode switch
        {
            HttpStatusCode.Conflict => Result.Fail<string>(conflictMessage),
            _ => Result.Fail($"Error {putRes.StatusCode}")
        };
    }

    public async Task<IResult<string>> UpdateUsernameAsync(string userID, string userName, string userJson, string adminToken, CancellationToken ct = default)
    {
        return await UpdateUserAsync
        (
            userID,
            userJson,
            adminToken,
            dict =>
            {
                dict["username"] = userName;
                dict["attributes"] = new Dictionary<string, string[]>
                {
                    ["profileCompleted"] = new[] { "true" },
                    ["avatarPath"] = new[] { _currentUser.AvatarPath }
                };
            },
            conflictMessage: "Username is taken.",
            ct
        );
    }

    public async Task<IResult<string>> UpdateAvatarAsync(string userID, string avatarPath, string userJson, string adminToken, CancellationToken ct = default)
    {
        return await UpdateUserAsync
        (
            userID,
            userJson,
            adminToken,
            dict =>
            {
                dict["username"] = _currentUser.Username;
                dict["attributes"] = new Dictionary<string, string[]>
                {
                    ["profileCompleted"] = new[] { "true" },
                    ["avatarPath"] = new[] { avatarPath }
                };
            },
            conflictMessage: "Avatar is taken.",
            ct
        );
    }
}