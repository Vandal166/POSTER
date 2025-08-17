namespace Domain.Entities;

public sealed class ConversationUser
{
    public Guid ConversationID { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public Guid UserID { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime JoinedAt { get; set; }
}