using FluentResults;

namespace Domain.Entities;

public sealed class ConversationUser
{
    public Guid ConversationID { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public Guid UserID { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime JoinedAt { get; set; }
}


// an conversation will hold list of all messages between users
public sealed class Conversation : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public Guid ProfilePictureID { get; private set; } // the profile picture ID from Blob Storage
    
    public Guid? AdminID { get; private set; } // the user who created the conversation, null if its a 1v1 conversation
    public User? Admin { get; private set; }
    
    private readonly List<ConversationUser> _participants = new();
    public IReadOnlyCollection<ConversationUser> Participants => _participants;
    
    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages;

    private Conversation() {}
    
    public static Result<Conversation> Create(string name, Guid profilePictureID, Guid? adminID = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail<Conversation>("Conversation name cannot be empty.");

        if (profilePictureID == Guid.Empty)
            return Result.Fail<Conversation>("Profile picture path cannot be empty.");
        
        var conversation = new Conversation
        {
            ID = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Name = name,
            ProfilePictureID = profilePictureID,
            AdminID = adminID
        };

        return Result.Ok(conversation);
    }
}

public sealed class Message : AuditableEntity
{
    public Guid SenderID { get; private set; } // Keycloak ID of the sender
    public User Sender { get; private set; } = null!;
    
    public Guid ConversationID { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    
    public string Content { get; private set; } = null!;
    public Guid? VideoFileID { get; private set; } // Optional video file ID for the msg
    
    // EF will populate this
    public IReadOnlyCollection<MessageImage> Images => _images;
    private readonly List<MessageImage> _images = new();
    
    private Message() {}
    
    public static Result<Message> Create(Guid senderID, Guid conversationID, string content, Guid? videoFileID = null)
    {
        if (senderID == Guid.Empty || conversationID == Guid.Empty)
            return Result.Fail<Message>("Sender and recipient IDs cannot be empty.");
        
        var message = new Message
        {
            ID        = Guid.NewGuid(),
            SenderID  = senderID,
            ConversationID = conversationID,
            Content   = content,
            CreatedAt = DateTime.UtcNow,
            VideoFileID = videoFileID
        };
        
        return Result.Ok(message);
    }
}