using System.Security.Claims;
using Application.Contracts;
using Application.DTOs;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;

namespace Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IAdminTokenProvider _tokenProv;
    private readonly IKeycloakUserService _kcUserService;
    private readonly IPasswordTokenProvider _passwordProvider;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly IValidator<CompleteProfileDto> _completeProfileValidator;
    private readonly IUserSynchronizer _userSync;
    
    public AuthService(IAdminTokenProvider tokenProv, IKeycloakUserService kcUserService, IPasswordTokenProvider passwordProvider, 
        IValidator<RegisterUserDto> registerValidator, IValidator<LoginUserDto> loginValidator, IValidator<CompleteProfileDto> completeProfileValidator, IUserSynchronizer userSynchronizer)
    {
        _tokenProv = tokenProv;
        _kcUserService = kcUserService;
        _passwordProvider = passwordProvider;
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
        var adminToken = await _tokenProv.GetTokenAsync(ct);
        
        var result = await _kcUserService.CreateAsync(dto, adminToken, ct);
        if (result.IsFailed)
            return Result.Fail(result.Errors.Select(e => e.Message));
        
        // Ok, then we also need to insert the same user of Keycloak into our local database
        var keycloakUser = await _kcUserService.GetUserAsync(result.Value, ct);
        if (keycloakUser is null)
            return Result.Fail("Unable to fetch user profile after registration.");

        // syncing to local db
        await _userSync.SyncAsync(keycloakUser.ID, ct);
                
        return Result.Ok();
    }
    
    public async Task<Result<(ClaimsPrincipal Principal, AuthenticationProperties Props)>> LoginAsync(LoginUserDto dto, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        
        // exchanging login/pwd for tokens + userID
        var tokenResult = await _passwordProvider.ExchangeAsync(dto.Login, dto.Password, ct);
        if (tokenResult.IsFailed)
            return Result.Fail(tokenResult.Errors.Select(e => e.Message));

        var tokens = tokenResult.Value;

        var kcUser = await _kcUserService.GetUserAsync(tokens.UserId, ct);
        if (kcUser is null)
            return Result.Fail("Unable to load user profile.");
        
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
        var adminToken = await _tokenProv.GetTokenAsync(ct);
        
        var userJson = await _kcUserService.GetAsync(userID, adminToken, ct);
        
        var updateRes = await _kcUserService.UpdateAsync(userID, dto.Username, userJson.Value, adminToken, ct);
        
        if (updateRes.IsFailed)
            return Result.Fail(updateRes.Errors.Select(e => e.Message));
        
        // syncing to local db
        await _userSync.SyncAsync(userID, ct);
        return Result.Ok();
    }
}