using FluentResults;

namespace Domain.Entities;

public sealed class Notification : AuditableEntity
{
    public Guid UserID { get; private set; } // the user who will receive the notification
    public User User { get; private set; } = null!;
    public string Message { get; private set; } = null!; // the message of the notification
    public string? WithRedirectUrl { get; private set; } // optional URL to redirect the user when they click on the notification
    public bool IsRead { get; private set; } // whether the notification has been read or not

    private Notification() { }

    public static Result<Notification> Create(Guid userID, string message, string? withRedirectUrl = null)
    {
        if (userID == Guid.Empty)
            return Result.Fail<Notification>("User ID is required.");
        
        if (string.IsNullOrWhiteSpace(message))
            return Result.Fail<Notification>("Message cannot be empty.");

        var notification = new Notification
        {
            ID = Guid.NewGuid(),
            UserID = userID,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            WithRedirectUrl = withRedirectUrl,
            IsRead = false
        };
        return Result.Ok(notification);
    }
}