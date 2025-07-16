using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(4);
    }
}