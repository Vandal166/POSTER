using FluentResults;

namespace Domain.Entities;

// an conversation will hold list of all messages between users

public sealed class Message : AuditableEntity
{
    public Guid SenderID { get; private set; } // Keycloak ID of the sender
    public User Sender { get; private set; } = null!;
    
    public Guid ConversationID { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    
    public string Content { get; private set; } = null!;
    public Guid? VideoFileID { get; private set; } // Optional video file ID for the msg
    
    public static readonly Guid SystemUserId = new Guid("11111111-1111-1111-1111-111111111111");
    
    public bool IsSystemMessage => SenderID == SystemUserId;
    
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
    
    public static Result<Message> CreateSystemMessage(Guid conversationID, string content)
    {
        if (conversationID == Guid.Empty)
            return Result.Fail<Message>("Conversation ID cannot be empty.");
        
        var message = new Message
        {
            ID = Guid.NewGuid(),
            SenderID = SystemUserId,
            ConversationID = conversationID,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        
        return Result.Ok(message);
    }
}