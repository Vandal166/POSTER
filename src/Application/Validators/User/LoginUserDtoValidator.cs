using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator()
    {
        RuleFor(x => x.Login).NotEmpty()
            .WithMessage("Login cannot be empty.");
        RuleFor(x => x.Password).NotEmpty()
            .WithMessage("Password cannot be empty.");
    }
}