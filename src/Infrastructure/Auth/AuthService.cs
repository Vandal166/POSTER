using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.Contracts;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;
public record TokenResponse(string AccessToken, string? IdToken, string RefreshToken, string UserId);

public interface IPasswordTokenProvider
{
    /// <summary>
    /// Exchanges username/password for tokens and extracts the userId ("sub").
    /// </summary>
    Task<IResult<TokenResponse>> ExchangeAsync(
        string login,
        string password,
        CancellationToken ct = default);
}
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
        if (string.IsNullOrEmpty(userID))
            return Result.Fail<TokenResponse>("Unable to load user profile.");

        return Result.Ok(new TokenResponse(accessToken, idToken, refreshToken, userID));
    }
    
    private static string GetFriendlyMessage(string body)
    {
        return JsonDocument.Parse(body).RootElement.GetProperty("error_description").GetString() ?? "An error occurred.";
    }
}

public interface IKeycloakUserCreator
{
    /// <summary>
    /// Creates a new Keycloak user and returns its Keycloak-generated ID.
    /// </summary>
    Task<IResult<string>>  CreateAsync(RegisterUserDto dto, string adminToken, CancellationToken ct = default);

    Task<IResult<string>> GetAsync(string userID, string adminToken, CancellationToken ct = default);
    
    Task<IResult<string>> UpdateAsync(string userID, string userName, string userJson, string adminToken, CancellationToken ct = default);
}

public class KeycloakUserCreator : IKeycloakUserCreator
{
    private readonly IHttpClientFactory _http;
    private readonly IKeycloakService _keycloakService;
    private readonly ICurrentUserService _currentUser;
    private readonly ITokenGenerator<HttpResponseMessage> _tokenGen;
    private readonly IConfiguration _config;

    public KeycloakUserCreator(IHttpClientFactory http, IKeycloakService keycloakService, ICurrentUserService currentUser,
                               ITokenGenerator<HttpResponseMessage> tokenGen,
                               IConfiguration config)
    {
        _http = http;
        _keycloakService = keycloakService;
        _currentUser = currentUser;
        _tokenGen = tokenGen;
        _config = config;
    }

    public async Task<IResult<string>> CreateAsync(RegisterUserDto dto, string adminToken, CancellationToken ct = default)
    {
        var keyc   = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");

        var userPayload = KeycloakClientFactory.CreateKeycloakUser(dto);
        var req = new HttpRequestMessage(HttpMethod.Post,
            $"/admin/realms/{keyc["Realm"]}/users")
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
        var keyc   = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");

        // getting the existing user
        var getReq = new HttpRequestMessage
        (
            HttpMethod.Get,
            $"/admin/realms/{keyc["Realm"]}/users/{userID}"
        );
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var getRes =  await _tokenGen.SendRequestAsync(client, getReq, ct);
        
        if (getRes is null || !getRes.IsSuccessStatusCode)
            return Result.Fail<string>($"Cannot fetch user: {getRes?.StatusCode}");

        var userJson = await getRes.Content.ReadAsStringAsync(ct);
        return Result.Ok(userJson);
    }

    public async Task<IResult<string>> UpdateAsync(string userID, string userName, string userJson, string adminToken, CancellationToken ct = default)
    {
        var keyc   = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");
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
            $"/admin/realms/{keyc["Realm"]}/users/{userID}"
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
        var putRes = await _tokenGen.SendRequestAsync(client, putReq, ct);

        if (putRes.IsSuccessStatusCode)
        {
            // updating the user claims
            var user = await _keycloakService.GetUserAsync(userID, ct);
            if (user is null)
                return Result.Fail<string>("Unable to fetch user profile after update.");
            var newPrincipal = ClaimsPrincipalFactory.BuildClaims(user);
            
            await _currentUser.RefreshClaims(newPrincipal);
            return Result.Ok(userID);
        }
        
        return putRes.StatusCode switch
        {
            HttpStatusCode.Conflict => Result.Fail<string>("Username is taken."),
            _ => Result.Fail($"Error: {putRes.StatusCode}")
        };
    }
}


public interface IAdminTokenProvider
{
    /// <summary>
    /// Returns a valid Keycloak admin access token.
    /// </summary>
    Task<string> GetTokenAsync(CancellationToken ct = default);
}

public class AdminTokenProvider : IAdminTokenProvider
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

public interface IUserSynchronizer
{
    /// <summary>
    /// Ensures the Keycloak user exists in the local database.
    /// </summary>
    Task SyncAsync(string keycloakID, CancellationToken ct = default);
}

public class UserSynchronizer : IUserSynchronizer
{
    private readonly IKeycloakService _kc;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public UserSynchronizer(IKeycloakService kc, IUserRepository users, IUnitOfWork uow)
    {
        _kc = kc;
        _users = users;
        _uow = uow;
    }

    public async Task SyncAsync(string keycloakID, CancellationToken ct = default)
    {
        var kcUser = await _kc.GetUserAsync(keycloakID, ct)
                     ?? throw new InvalidOperationException("Keycloak user not found.");

        if (!Guid.TryParse(kcUser.ID, out var id))
            throw new InvalidOperationException("Invalid Keycloak user ID.");

        var existing = await _users.GetUserAsync(id, ct);
        if (existing is null)
        {
            var user = User.Create(id, kcUser.Username).Value;
            await _users.AddAsync(user); 
        }
        else // if the user exists then we update him to reflect Keycloak changes
        {
            existing.Username = kcUser.Username!;
            await _users.UpdateAsync(existing);
        }

        await _uow.SaveChangesAsync(ct);
    }
}


public class AuthService : IAuthService
{
    private readonly IAdminTokenProvider _tokenProv;
    private readonly IKeycloakUserCreator _kcUserCreator;
    private readonly IPasswordTokenProvider _passwordProvider;
    private readonly IKeycloakService _keycloakService;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly IValidator<CompleteProfileDto> _completeProfileValidator;
    private readonly IUserSynchronizer _userSync;
    
    public AuthService(IAdminTokenProvider tokenProv, IKeycloakUserCreator kcUserCreator, IPasswordTokenProvider passwordProvider, IKeycloakService keycloakService, 
        IValidator<RegisterUserDto> registerValidator, IValidator<LoginUserDto> loginValidator, IValidator<CompleteProfileDto> completeProfileValidator, IUserSynchronizer userSynchronizer)
    {
        _tokenProv = tokenProv;
        _kcUserCreator = kcUserCreator;
        _passwordProvider = passwordProvider;
        _keycloakService = keycloakService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _completeProfileValidator = completeProfileValidator;
        _userSync = userSynchronizer;
    }

    public async Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
    {
        // dto validation
        var validation = await _registerValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        // getting an admin token via client_credentials
        /*var client = _http.CreateClient("Keycloak");
        var keyc = _config.GetSection("Keycloak");
        
        var formUrlContent = ROPTokenGeneratorService.CreateFormContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "client_credentials",
            ["client_id"]     = keyc["AdminClientId"],
            ["client_secret"] = keyc["AdminClientSecret"]
        });
        var tokenRes = await _ropTokenGenerator.GenerateTokenAsync(client, formUrlContent, ct);
        if (!tokenRes.IsSuccessStatusCode)
        {
            var body = await tokenRes.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"Token {tokenRes.Headers} body {body}");
            return Result.Fail(GetFriendlyMessage(body));
        }

        var adminToken = JsonDocument.Parse(await tokenRes.Content.ReadAsStringAsync(ct))
            .RootElement.GetProperty("access_token").GetString();*/
        var adminToken = await _tokenProv.GetTokenAsync(ct);
        
        /*var user = KeycloakClientFactory.CreateKeycloakUser(dto);
        
        var req = new HttpRequestMessage(HttpMethod.Post, 
            $"/admin/realms/{keyc["Realm"]}/users")
        {
            Content = new StringContent(JsonSerializer.Serialize(user),
                Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = 
            new AuthenticationHeaderValue("Bearer", adminToken);

        var createRes = await _ropTokenGenerator.SendRequestAsync(client, req, ct);*/
        var result = await _kcUserCreator.CreateAsync(dto, adminToken, ct);
        if (result.IsFailed)
            return Result.Fail(result.Errors.Select(e => e.Message));
        
        // Ok, then we also need to insert the same user of Keycloak into our local database
        var keycloakUser = await _keycloakService.GetUserAsync(result.Value, ct);
        if (keycloakUser is null)
            return Result.Fail("Unable to fetch user profile after registration.");

        // syncing to local db
        await _userSync.SyncAsync(keycloakUser.ID, ct);
                
        return Result.Ok();
        /*switch (createRes.StatusCode)
        {
            case HttpStatusCode.Created:
            {
                // Ok, then we also need to insert the same user of Keycloak into our local database
                var keycloakUser = await _keycloakService.GetUserAsync(createRes.Headers.Location.Segments.Last(), ct);
                if (keycloakUser is null)
                    return Result.Fail("Unable to fetch user profile after registration.");

                // syncing to local db
                await _userSync.SyncAsync(keycloakUser.ID, ct);
                
                return Result.Ok();
            }
            case HttpStatusCode.Conflict:
                return Result.Fail("User already exists.");
            default:
                return Result.Fail($"Error: {createRes.StatusCode}");
        }*/
    }
    
    public async Task<Result<(ClaimsPrincipal Principal, AuthenticationProperties Props)>> LoginAsync(LoginUserDto dto, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        /*var keyc = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");
        var formUrlContent = ROPTokenGeneratorService.CreateFormContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "password",
            ["client_id"]     = keyc["ClientId"],
            ["client_secret"] = keyc["ClientSecret"],
            ["username"]      = dto.Login,
            ["password"]      = dto.Password
        });
        var tokenRes = await _ropTokenGenerator.GenerateTokenAsync(client, formUrlContent, ct);
        if (!tokenRes.IsSuccessStatusCode)
        {
            var body = await tokenRes.Content.ReadAsStringAsync(ct);
            return Result.Fail(GetFriendlyMessage(body));
        }
        Console.WriteLine($"Token {tokenRes.Headers} body {await tokenRes.Content.ReadAsStringAsync(ct)}");
        
        var (idToken, accessToken, refreshToken) = await ROPTokenGeneratorService.ParseTokenResponseAsync(tokenRes);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        var userID = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var user = await _keycloakService.GetUserAsync(userID, ct);
        if (user == null)
            return Result.Fail("Unable to load user profile.");*/
// 1) Exchange username/password for tokens + userId
        var tokenResult = await _passwordProvider.ExchangeAsync(dto.Login, dto.Password, ct);
        if (tokenResult.IsFailed)
            return Result.Fail(tokenResult.Errors.Select(e => e.Message));

        var tokens = tokenResult.Value;

        // 2) Fetch full Keycloak user (with attributes/roles)
        var kcUser = await _keycloakService.GetUserAsync(tokens.UserId, ct);
        if (kcUser is null)
            return Result.Fail("Unable to load user profile from Keycloak.");
        
        // building a ClaimsPrincipal from the user object
        var userPrincipal = ClaimsPrincipalFactory.BuildClaims(kcUser);
        
        // buildign the cookie, persisting the tokens
        var props = CookiePropertiesFactory.BuildCookie(tokens.AccessToken, tokens.RefreshToken, tokens.IdToken);
        
        return Result.Ok((userPrincipal, props));
    }

    public async Task<Result> CompleteProfileAsync(string userID, CompleteProfileDto dto, CancellationToken ct = default)
    {
        var validation = await _completeProfileValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        
        // gettin an admin token via client_credentials
        /*var client = _http.CreateClient("Keycloak");
        var keyc = _config.GetSection("Keycloak");

        var formUrlContent = ROPTokenGeneratorService.CreateFormContent
        (
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = keyc["AdminClientId"],
                ["client_secret"] = keyc["AdminClientSecret"]
            }
        );
        var tokenRes = await _ropTokenGenerator.GenerateTokenAsync(client, formUrlContent, ct);
        if (!tokenRes.IsSuccessStatusCode)
        {
            var body = await tokenRes.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"Token {tokenRes.Headers} body {body}");
            return Result.Fail(GetFriendlyMessage(body));
        }

        var adminToken = JsonDocument.Parse(await tokenRes.Content.ReadAsStringAsync(ct))
            .RootElement.GetProperty("access_token").GetString();*/

        var adminToken = await _tokenProv.GetTokenAsync(ct);
        /*var getRes = await KeycloakClientFactory.GetKeycloakUser(userID, client, _ropTokenGenerator, adminToken, keyc, ct);

        if (getRes is null || !getRes.IsSuccessStatusCode)
            return Result.Fail($"Cannot fetch user: {getRes?.StatusCode}");

        var userJson = await getRes.Content.ReadAsStringAsync(ct);*/
        
        var userJson = await _kcUserCreator.GetAsync(userID, adminToken, ct);

        /*var putRes = await KeycloakClientFactory.SetKeycloakUsername(userID, dto.Username, userJson.Value, client, _ropTokenGenerator, adminToken, keyc, ct);
        if (putRes.IsSuccessStatusCode)
            return Result.Ok();

        return putRes.StatusCode switch
        {
            HttpStatusCode.Conflict => Result.Fail("Username is taken."),
            _ => Result.Fail($"Error: {putRes.StatusCode}")
        };*/
        
        var updateRes = await _kcUserCreator.UpdateAsync(userID, dto.Username, userJson.Value, adminToken, ct);
        
        if (updateRes.IsFailed)
            return Result.Fail(updateRes.Errors.Select(e => e.Message));
        
        // syncing to local db
        await _userSync.SyncAsync(userID, ct);
        return Result.Ok();
    }
}