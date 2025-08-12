using FluentResults;

namespace Domain.Entities;

public sealed class Conversation : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public Guid ProfilePictureID { get; private set; } // the profile picture ID from Blob Storage

    public Guid CreatedByID { get; private set; } // the user who administers this conversation
    public User CreatedBy { get; private set; } = null!;
    
    private readonly List<ConversationUser> _participants = new();
    public IReadOnlyCollection<ConversationUser> Participants => _participants;
    
    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages;

    private Conversation() {}
    
    public static Result<Conversation> Create(string name, Guid profilePictureID, Guid createdByID)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail<Conversation>("Conversation name cannot be empty.");

        if (profilePictureID == Guid.Empty)
            return Result.Fail<Conversation>("Profile picture path cannot be empty.");
        
        if (createdByID == Guid.Empty)
            return Result.Fail<Conversation>("Created by user ID cannot be empty.");
        
        var conversation = new Conversation
        {
            ID = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Name = name,
            ProfilePictureID = profilePictureID,
            CreatedByID = createdByID
        };
        return Result.Ok(conversation);
    }
}