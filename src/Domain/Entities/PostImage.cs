namespace Domain.Entities;

public class PostImage : AuditableEntity
{
    public Guid PostID { get; set; }
    public Guid ImageFileID { get; set; }
    public Post Post { get; set; } = null!;
}