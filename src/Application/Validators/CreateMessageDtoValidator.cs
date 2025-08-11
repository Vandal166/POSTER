using Application.Contracts.Persistence;
using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public sealed class CreateMessageDtoValidator : AbstractValidator<CreateMessageDto>
{
    public CreateMessageDtoValidator(IConversationRepository conversationRepo)
    {
        RuleFor(c => c.ConversationID).MustAsync(async (id, ct) 
            => await conversationRepo.ExistsAsync(id, ct))
            .WithMessage("Conversation does not exist.");
        
        RuleFor(c => c.Content)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(500).WithMessage("Message must be between 2 and 500 characters long.");
    }
}