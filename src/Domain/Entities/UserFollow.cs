namespace Domain.Entities;

public sealed class UserFollow
{
    public Guid FollowerID { get; set; } // User who is following
    public User Follower { get; set; } = null!;

    public Guid FollowedID { get; set; } // User who is being followed
    public User Followed { get; set; } = null!;

    public DateTime FollowedAt { get; set; }
}