using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(1800).WithMessage("Post content must be between 4 and 1800 characters long.");
    }
}