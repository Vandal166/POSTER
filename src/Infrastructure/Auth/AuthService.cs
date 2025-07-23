using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.Contracts;
using Application.DTOs;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _http;
    private readonly ITokenGenerator<HttpResponseMessage> _ropTokenGenerator;
    private readonly IKeycloakService _keycloakService;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly IValidator<CompleteProfileDto> _completeProfileValidator;
    private readonly IConfiguration _config;
    
    public AuthService(IHttpClientFactory http, ITokenGenerator<HttpResponseMessage> ropTokenGenerator, IKeycloakService keycloakService, IValidator<RegisterUserDto> registerValidator, IValidator<LoginUserDto> loginValidator,
        IValidator<CompleteProfileDto> completeProfileValidator, IConfiguration configuration)
    {
        _http = http;
        _ropTokenGenerator = ropTokenGenerator;
        _keycloakService = keycloakService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _completeProfileValidator = completeProfileValidator;
        _config = configuration;
    }

    public async Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
    {
        // dto validation
        var validation = await _registerValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        // getting an admin token via client_credentials
        var keyc = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");
        
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
            .RootElement.GetProperty("access_token").GetString();
        
        var user = KeycloakClientFactory.CreateKeycloakUser(dto);
        
        var req = new HttpRequestMessage(HttpMethod.Post, 
            $"/admin/realms/{keyc["Realm"]}/users")
        {
            Content = new StringContent(JsonSerializer.Serialize(user),
                Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = 
            new AuthenticationHeaderValue("Bearer", adminToken);

        var createRes = await _ropTokenGenerator.SendRequestAsync(client, req, ct);
        return createRes.StatusCode switch
        {
            HttpStatusCode.Created => Result.Ok(),
            HttpStatusCode.Conflict => Result.Fail("User already exists."),
            _ => Result.Fail($"Error: {createRes.StatusCode}")
        };
    }
    
    public async Task<Result<(ClaimsPrincipal Principal, AuthenticationProperties Props)>> LoginAsync(LoginUserDto dto, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        var keyc = _config.GetSection("Keycloak");
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
            return Result.Fail("Unable to load user profile.");

        // building a ClaimsPrincipal from the user object
        var userPrincipal = ClaimsPrincipalFactory.BuildClaims(user);
        
        // buildign the cookie, persisting the tokens
        var props = CookiePropertiesFactory.BuildCookie(accessToken, refreshToken, idToken);
        
        return Result.Ok((userPrincipal, props));
    }

    public async Task<Result> CompleteProfileAsync(string userID, CompleteProfileDto dto, CancellationToken ct = default)
    {
        var validation = await _completeProfileValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        
        // gettin an admin token via client_credentials
        var keyc = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");

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
            .RootElement.GetProperty("access_token").GetString();

        var getRes = await KeycloakClientFactory.GetKeycloakUser(userID, client, _ropTokenGenerator, adminToken, keyc, ct);

        if (getRes is null || !getRes.IsSuccessStatusCode)
            return Result.Fail($"Cannot fetch user: {getRes?.StatusCode}");

        var userJson = await getRes.Content.ReadAsStringAsync(ct);

        var putRes = await KeycloakClientFactory.SetKeycloakUsername(userID, dto.Username, userJson, client, _ropTokenGenerator, adminToken, keyc, ct);
        if (putRes.IsSuccessStatusCode)
            return Result.Ok();

        return putRes.StatusCode switch
        {
            HttpStatusCode.Conflict => Result.Fail("Username is taken."),
            _ => Result.Fail($"Error: {putRes.StatusCode}")
        };
    }


    private static string GetFriendlyMessage(string body)
    {
        return JsonDocument.Parse(body).RootElement.GetProperty("error_description").GetString() ?? "An error occurred.";
    }
}