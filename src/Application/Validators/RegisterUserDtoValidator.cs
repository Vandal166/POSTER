using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().Length(3, 50);
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(6);
    }
}