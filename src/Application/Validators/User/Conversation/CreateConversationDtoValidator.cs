using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CreateConversationDtoValidator : AbstractValidator<CreateConversationDto>
{
    public CreateConversationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(40).WithMessage("Conversation name must be between 1 and 40 characters long.");
    }
}