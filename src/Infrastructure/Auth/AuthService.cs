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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _http;
    private readonly ITokenGenerator<HttpResponseMessage> _ropTokenGenerator;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly IConfiguration _config;
    
    public AuthService(IHttpClientFactory http, ITokenGenerator<HttpResponseMessage> ropTokenGenerator, IValidator<RegisterUserDto> registerValidator, IValidator<LoginUserDto> loginValidator,
        IConfiguration configuration)
    {
        _http = http;
        _ropTokenGenerator = ropTokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _config = configuration;
    }

    public async Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        // dto validation
        var validation = await _registerValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        // getting an admin token via client_credentials
        var keyc = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");

        /*var tokenRes = await client.PostAsync(
            "protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string> {
                ["grant_type"]    = "client_credentials",
                ["client_id"]     = keyc["AdminClientId"],
                ["client_secret"] = keyc["AdminClientSecret"]
            }),
            cancellationToken
        );*/
        var formUrlContent = ROPTokenGeneratorService.CreateFormContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "client_credentials",
            ["client_id"]     = keyc["AdminClientId"],
            ["client_secret"] = keyc["AdminClientSecret"]
        });
        var tokenRes = await _ropTokenGenerator.GenerateTokenAsync(client, formUrlContent, cancellationToken);
        if (!tokenRes.IsSuccessStatusCode)
        {
            Console.WriteLine($"Token {tokenRes.Headers} body {await tokenRes.Content.ReadAsStringAsync()}");
            return Result.Fail("Cannot acquire admin token.");
        }

        var adminToken = JsonDocument.Parse(await tokenRes.Content.ReadAsStringAsync(cancellationToken))
            .RootElement.GetProperty("access_token").GetString();
        
        var user = KeycloakClientService.CreateClient(dto);
        
        var req = new HttpRequestMessage(HttpMethod.Post, 
            $"/admin/realms/{keyc["Realm"]}/users")
        {
            Content = new StringContent(JsonSerializer.Serialize(user),
                Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = 
            new AuthenticationHeaderValue("Bearer", adminToken);

        /*var createRes = await client.SendAsync(req, cancellationToken);*/
        var createRes = await _ropTokenGenerator.SendRequestAsync(client, req, cancellationToken);
        return createRes.StatusCode switch
        {
            HttpStatusCode.Created => Result.Ok(),
            HttpStatusCode.Conflict => Result.Fail("User already exists."),
            _ => Result.Fail($"Keycloak error: {createRes.StatusCode}")
        };
    }
    
    public async Task<Result<(ClaimsPrincipal Principal, AuthenticationProperties Props)>> LoginAsync(LoginUserDto dto, HttpContext httpContext)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        var keyc = _config.GetSection("Keycloak");
        var formUrlContent = ROPTokenGeneratorService.CreateFormContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "password",
            ["client_id"]     = keyc["ClientId"],
            ["client_secret"] = keyc["ClientSecret"],
            ["username"]      = dto.Username,
            ["password"]      = dto.Password
        });
        var tokenRes = await _ropTokenGenerator.GenerateTokenAsync(_http.CreateClient("Keycloak"), formUrlContent);
        if (!tokenRes.IsSuccessStatusCode)
        {
            Console.WriteLine($"Token {tokenRes.Headers} body {await tokenRes.Content.ReadAsStringAsync()}");
            return Result.Fail("Invalid username or password.");
        }
        Console.WriteLine($"Token {tokenRes.Headers} body {await tokenRes.Content.ReadAsStringAsync()}");
        /*var json = JsonDocument.Parse(await tokenRes.Content.ReadAsStringAsync()).RootElement;
        var idToken      = json.GetProperty("id_token").GetString();
        var accessToken  = json.GetProperty("access_token").GetString();
        var refreshToken = json.GetProperty("refresh_token").GetString();*/
        var (idToken, accessToken, refreshToken) = await ROPTokenGeneratorService.ParseTokenResponeAsync(tokenRes);
        
        // building a ClaimsPrincipal from the ID token
        /*var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(idToken);
        var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var userPrincipal = new ClaimsPrincipal(identity);*/
        var userPrincipal = KeycloakClientService.BuildClaims(idToken ?? accessToken);
        
        // signing in with cookie, persisting the tokens
        /*var props = new AuthenticationProperties(new Dictionary<string, string> {
                [".Token.access_token"]  = accessToken,
                [".Token.refresh_token"] = refreshToken,
                [".Token.id_token"]      = idToken
            })
            { IsPersistent = true };*/

        var props = KeycloakClientService.BuildCookie(accessToken, refreshToken, idToken);
        //await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, props);

        return Result.Ok((userPrincipal, props));
    }

    public async Task<Result> CompleteProfileAsync(string userID, string userName)
    {
        // 1) Get an admin token via client_credentials
        var keyc = _config.GetSection("Keycloak");
        var client = _http.CreateClient("Keycloak");

        /*var tokenRes = await client.PostAsync(
            "protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string> {
                ["grant_type"]    = "client_credentials",
                ["client_id"]     = keyc["AdminClientId"],
                ["client_secret"] = keyc["AdminClientSecret"]
            }));
            */

        var formUrlContent = ROPTokenGeneratorService.CreateFormContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "client_credentials",
            ["client_id"]     = keyc["AdminClientId"],
            ["client_secret"] = keyc["AdminClientSecret"]
        });
        var tokenRes = await _ropTokenGenerator.GenerateTokenAsync(client, formUrlContent);
        if (!tokenRes.IsSuccessStatusCode)
        {
            Console.WriteLine($"Token {tokenRes.Headers} body {await tokenRes.Content.ReadAsStringAsync()}");
            return Result.Fail("Cannot acquire admin token.");
        }

        var adminToken = JsonDocument.Parse(await tokenRes.Content.ReadAsStringAsync())
            .RootElement.GetProperty("access_token").GetString();

        // 2) GET the existing user
        var getReq = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/{keyc["Realm"]}/users/{userID}"
        );
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var getRes = await _ropTokenGenerator.SendRequestAsync(client, getReq);
        //var getRes = await client.SendAsync(getReq);
        if (!getRes.IsSuccessStatusCode)
            return Result.Fail($"Cannot fetch user: {getRes.StatusCode}");

        var userJson = await getRes.Content.ReadAsStringAsync();

        // 3) Build a mutable dictionary from the JSON
        var dict = JsonSerializer.Deserialize<Dictionary<string,object>>(userJson)!;

        // 4) Apply your updates
        dict["username"]   = userName;
        dict["attributes"] = new Dictionary<string,string[]>
        {
            ["profileCompleted"] = new[] { "true" }
        };

        // 5) PUT the full object back
        var putReq = new HttpRequestMessage(
            HttpMethod.Put,
            $"/admin/realms/{keyc["Realm"]}/users/{userID}"
        )
        {
            Content = new StringContent(
                JsonSerializer.Serialize(dict),
                Encoding.UTF8,
                "application/json"
            )
        };
        putReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var putRes = await _ropTokenGenerator.SendRequestAsync(client, putReq);//.SendAsync(putReq);
        if (putRes.IsSuccessStatusCode)
            return Result.Ok();
        
        return Result.Fail($"Keycloak error: {putRes.StatusCode}");
    }
}