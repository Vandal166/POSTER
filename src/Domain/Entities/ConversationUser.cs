namespace Domain.Entities;

public sealed class ConversationUser
{
    public Guid ConversationID { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public Guid UserID { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime JoinedAt { get; set; }
}

public sealed class UserFollow
{
    public Guid FollowerID { get; set; } // User who is following
    public User Follower { get; set; } = null!;

    public Guid FollowedID { get; set; } // User who is being followed
    public User Followed { get; set; } = null!;

    public DateTime FollowedAt { get; set; }
}

public sealed class UserBlock
{
    public Guid BlockerID { get; set; }
    public User Blocker { get; set; } = null!;

    public Guid BlockedID { get; set; }
    public User Blocked { get; set; } = null!;

    public DateTime BlockedAt { get; set; }
}