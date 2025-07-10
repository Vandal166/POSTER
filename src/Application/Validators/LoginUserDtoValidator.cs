using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}