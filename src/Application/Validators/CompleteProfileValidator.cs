using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CompleteProfileValidator : AbstractValidator<CompleteProfileDto>
{
    public CompleteProfileValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .Matches(@"^[a-zA-Z0-9_]+$");
    }
}