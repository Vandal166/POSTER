using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class UpdateUsernameValidator : AbstractValidator<UsernameDto>
{
    public UpdateUsernameValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters long.")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]{2,49}(?<!_)$").WithMessage("Username can only contain letters, numbers, and underscores.");
    }
}