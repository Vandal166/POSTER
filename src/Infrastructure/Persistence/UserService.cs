using Application.Contracts;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UsernameDto> _usernameValidator;
    private readonly IValidator<AvatarDto> _avatarValidator;
    private readonly IUserSynchronizer _userSync;
    
    public UserService(IAvatarService avatarService, IAuthService authService, IUserRepository userRepo, IUnitOfWork uow,
        IValidator<UsernameDto> usernameValidator, IValidator<AvatarDto> avatarValidator,
        IUserSynchronizer userSync)
    {
        _avatarService = avatarService;
        _authService = authService;
        _userRepository = userRepo;
        _unitOfWork = uow;
        _usernameValidator = usernameValidator;
        _avatarValidator = avatarValidator;
        _userSync = userSync;
    }

    public async Task<string> GetAvatarPathAsync(string userID, CancellationToken ct = default)
    {
        var user = await _userRepository.GetUserAsync(Guid.Parse(userID), ct);
        if (user is null)
            throw new InvalidOperationException("Could not find user.");
        
        return user.AvatarPath; // returns default avatar path if null
    }

    public async Task<Result> UpdateUsernameAsync(string userID, UsernameDto dto, CancellationToken ct = default)
    {
        var validation = await _usernameValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        
        var updateRes = await _authService.UpdateKeycloakProfileAsync(userID, dto, ct);
        if (updateRes.IsFailed)
            return Result.Fail(updateRes.Errors.Select(e => e.Message));
        
        // syncing to local db
        await _userSync.SyncAsync(userID, ct);
        return Result.Ok();
    }

    public async Task<Result<string>> UpdateAvatarAsync(string userID, AvatarDto fileDto, CancellationToken ct = default)
    {
        var validation = await _avatarValidator.ValidateAsync(fileDto, ct); //TODO (file == null || file.Length == 0)
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        
        var path =  await _avatarService.UpdateAvatarAsync(userID, fileDto, ct);
        if (string.IsNullOrEmpty(path))
            return Result.Fail("Failed to update avatar. Please try again later.");
        
        var user = await _userRepository.GetUserAsync(Guid.Parse(userID), ct);
        if (user is null)
            return Result.Fail($"Could not find user.");
        
        user.AvatarPath = path;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(path);
    }
}