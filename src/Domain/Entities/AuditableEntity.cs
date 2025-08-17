namespace Domain.Entities;

// an base entity that can be used to track creation, update, and soft deletion
public abstract class AuditableEntity
{
    public Guid ID { get; protected set; } // TODO an Snowflake ID would be better
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    public void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
    public void MarkDeleted() => DeletedAt = DateTime.UtcNow;
}