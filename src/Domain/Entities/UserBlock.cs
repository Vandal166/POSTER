namespace Domain.Entities;

public sealed class UserBlock
{
    public Guid BlockerID { get; set; }
    public User Blocker { get; set; } = null!;

    public Guid BlockedID { get; set; }
    public User Blocked { get; set; } = null!;

    public DateTime BlockedAt { get; set; }
}