using Application.Contracts.Persistence;
using Application.DTOs;
using FluentResults;
using FluentValidation;
using Infrastructure.Auth;

namespace Infrastructure.Persistence;

public sealed class UserService : IUserService
{
    private readonly IAvatarService _avatarService;
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UsernameDto> _usernameValidator;
    private readonly IValidator<AvatarDto> _avatarValidator;
    
    public UserService(IAvatarService avatarService, IAuthService authService, IUserRepository userRepo,
        IValidator<UsernameDto> usernameValidator, IValidator<AvatarDto> avatarValidator)
    {
        _avatarService = avatarService;
        _authService = authService;
        _userRepository = userRepo;
        _usernameValidator = usernameValidator;
        _avatarValidator = avatarValidator;
    }

    public async Task<Result<string>> GetAvatarPathAsync(string userID, CancellationToken ct = default)
    {
        var user = await _userRepository.GetUserAsync(Guid.Parse(userID), ct);
        if (user is null)
            return Result.Fail("User not found.");
        
        return user.AvatarPath; // returns default avatar path if null
    }

    public async Task<Result> UpdateUsernameAsync(string userID, UsernameDto dto, CancellationToken ct = default)
    {
        var validation = await _usernameValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        
        var updateRes = await _authService.UpdateKeycloakUsernameAsync(userID, dto, ct);
        return updateRes.IsFailed switch
        {
            true => Result.Fail(updateRes.Errors.Select(e => e.Message)),
            _ => Result.Ok()
        };
    }

    public async Task<Result> UpdateAvatarAsync(string userID, AvatarDto fileDto, CancellationToken ct = default)
    {
        var validation = await _avatarValidator.ValidateAsync(fileDto, ct); //TODO (file == null || file.Length == 0)
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        
        var path =  await _avatarService.UpdateAvatarAsync(userID, fileDto, ct);
        if (string.IsNullOrEmpty(path))
            return Result.Fail("Failed to update avatar. Please try again later.");
        
        var updateRes = await _authService.UpdateKeycloakAvatarAsync(userID, path, ct);// updating also on Keycloak side
        return updateRes.IsFailed switch
        {
            true => Result.Fail(updateRes.Errors.Select(e => e.Message)),
            _ => Result.Ok()
        };
    }
}