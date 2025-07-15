namespace Domain.Entities;

public abstract class AuditableEntity
{
    public Guid ID { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    public void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
    public void MarkDeleted() => DeletedAt = DateTime.UtcNow;
}