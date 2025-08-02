using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        //Need also change the front-end validation
        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(500).WithMessage("Comment must be between 4 and 500 characters long."); //TODO ด้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็็็้้้้้็็็็ bro what the hell is this character
    }
}