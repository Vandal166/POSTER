using Domain.Entities;
using FluentValidation;

namespace Application.Validators;

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(4);
    }
}