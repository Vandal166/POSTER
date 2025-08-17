using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class UpdateAvatarValidator : AbstractValidator<AvatarDto>
{
    public UpdateAvatarValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9_\-]+\.(jpg|jpeg|png|gif)$")
            .WithMessage("File name must be a valid image file name (jpg, jpeg, png, gif).");
        RuleFor(x => x.Content)
            .NotNull()
            .Must(content => content.CanSeek && content.Length > 0)
            .WithMessage("Content must be a non-empty stream.")
            .Must(content => content.CanSeek && content.Length <= 2 * 1024 * 1024)
            .WithMessage("Avatar file size must not exceed 2 MB.");
    }
}