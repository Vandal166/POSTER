using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public sealed class UpdateConversationDtoValidator : AbstractValidator<UpdateConversationDto>
{
    public UpdateConversationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(40).WithMessage("Conversation name must be between 1 and 40 characters long.");
    }
}