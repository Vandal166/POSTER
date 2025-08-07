using FluentResults;

namespace Domain.Entities;

public sealed class MessageImage : AuditableEntity
{
    public Guid MessageID { get; private set; }
    public Guid ImageFileID { get; private set; }
    public Message Message { get; private set; } = null!;
    
    private MessageImage() { }
    
    public static Result<MessageImage> Create(Guid messageID, Guid imageFileID)
    {
        if (messageID == Guid.Empty)
            return Result.Fail<MessageImage>("Message ID cannot be empty.");
        
        if (imageFileID == Guid.Empty)
            return Result.Fail<MessageImage>("Image File ID cannot be empty.");
        
        var messageImage = new MessageImage
        {
            ID = Guid.NewGuid(),
            MessageID = messageID,
            ImageFileID = imageFileID,
            CreatedAt = DateTime.UtcNow
        };
        
        return Result.Ok(messageImage);
    }
}