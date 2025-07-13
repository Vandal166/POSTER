using Application.Contracts;
using Application.DTOs;
using Domain.ValueObjects;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly IUserRepository           _users;
    private readonly IPasswordHasher          _hasher;
    private readonly IUnitOfWork              _uow;
    private readonly ITokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _jwtConfiguration;
    
    public AuthService(IValidator<RegisterUserDto> registerValidator, IValidator<LoginUserDto> loginValidator, IUserRepository users, 
        IPasswordHasher hasher, IUnitOfWork uow, ITokenGenerator tokenGenerator, IConfiguration configuration)
    {
        _registerValidator = registerValidator;
        _loginValidator    = loginValidator;
        _users     = users;
        _hasher    = hasher;
        _uow       = uow;
        _jwtTokenGenerator = tokenGenerator;
        _jwtConfiguration = configuration;
    }

    public async Task<Result<Guid>> RegisterAsync(RegisterUserDto dto)
    {
        // dto validation
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail<Guid>(validation.Errors.Select(e => e.ErrorMessage));

        // VO validation
        var userNameR = UserName.Create(dto.Username);
        var emailR    = Email.Create(dto.Email);

        var errors = userNameR.Errors.Concat(emailR.Errors).ToList();
        if (errors.Any())
            return Result.Fail<Guid>(errors.Select(e => e.Message));
        
        var user = new Domain.Entities.User
        {
            ID           = Guid.NewGuid(),
            Username     = userNameR.Value,
            Email        = emailR.Value,
            PasswordHash = _hasher.Hash(dto.Password)
        };

        // uniqueness check
        var uniqueErrors = (await _users.UserExistsAsync(user)).ToList();
        
        if (uniqueErrors.Any())
            return Result.Fail<Guid>(uniqueErrors.Select(e => e.Message));
        
        await _users.AddAsync(user);
        await _uow.SaveChangesAsync();
        return Result.Ok(user.ID);
    }

    public async Task<Result<string>> LoginAsync(LoginUserDto dto)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail<string>(validation.Errors.Select(e => e.ErrorMessage));

        var userNameR = UserName.Create(dto.Username);
        if (userNameR.IsFailed)
            return Result.Fail<string>(userNameR.Errors.Select(e => e.Message));

        var user = await _users.GetUserByLogin(userNameR.Value);
        if (user == null)
            return Result.Fail<string>("User does not exist");
        
        var passwordHash = _hasher.Hash(dto.Password);
        if(_hasher.Verify(dto.Password, passwordHash) == false)
            return Result.Fail<string>("Invalid password");

        //TODO abstract token generation behind an interface in the Application layer (e.g., ITokenGenerator) and inject it into 

        var jwtToken = _jwtTokenGenerator.GenerateToken(user, _jwtConfiguration);

        return Result.Ok(jwtToken);
    }
}