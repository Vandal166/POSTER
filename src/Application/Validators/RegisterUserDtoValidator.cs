using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(6);
    }
}