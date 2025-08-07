using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PosterDbContext : DbContext
{
    public PosterDbContext(DbContextOptions<PosterDbContext> options) : base(options)
    { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostImage> PostImages => Set<PostImage>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<PostView> PostViews => Set<PostView>();
    
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationUser> ConversationUsers => Set<ConversationUser>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageImage> MessageImages => Set<MessageImage>();
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
     
        // ------ User ------
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.ID);
            b.Property(u => u.Username).IsRequired();
            b.Property(u => u.AvatarPath).HasMaxLength(256).IsRequired();
            //b.Property(u => u.Email).IsRequired();
           // b.OwnsOne(u => u.Username, x => x.Property(p => p.Value).HasColumnName("Username").IsRequired());
           // b.OwnsOne(u => u.Email,    x => x.Property(p => p.Value).HasColumnName("Email").IsRequired());
           // b.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
            
            b.HasQueryFilter(u => u.DeletedAt == null); // soft delete filter
        });

        // ------ Post ------
        modelBuilder.Entity<Post>(b =>
        {
            b.HasKey(p => p.ID);
            b.Property(p => p.Content).IsRequired();
            b.HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorID)
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasIndex(p => p.CreatedAt);
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });
        
        // ------ Comment ------
        modelBuilder.Entity<Comment>(b =>
        {
            b.HasKey(c => c.ID);
            b.Property(c => c.Content).IsRequired();
            b.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostID)
                .OnDelete(DeleteBehavior.Cascade); // removing a post will remove its comments
            b.HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorID)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentID)
                .OnDelete(DeleteBehavior.Cascade); // removing a parent comment will remove its replies
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
            
            /*b.HasQueryFilter(c => c.ParentCommentID == null); // filter to only show top-level comments by default*/
        });
       
        // ------ PostLike ------
        modelBuilder.Entity<PostLike>(b =>
        {
            b.HasKey(pl => pl.ID);
            b.HasOne(pl => pl.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PostID)
                .OnDelete(DeleteBehavior.Cascade); // removing a post will remove its likes
            b.HasOne(pl => pl.User)
                .WithMany()
                .HasForeignKey(pl => pl.UserID);
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });
       
        // ------ CommentLike ------
        modelBuilder.Entity<CommentLike>(b =>
        {
            b.HasKey(cl => cl.ID);
            b.HasOne(cl => cl.Comment)
                .WithMany()
                .HasForeignKey(cl => cl.CommentID)
                .OnDelete(DeleteBehavior.Cascade); // removing a comment will remove its likes
            b.HasOne(cl => cl.User)
                .WithMany()
                .HasForeignKey(cl => cl.UserID);
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });
        
        // ------ PostView ------
        modelBuilder.Entity<PostView>(b =>
        {
            b.HasKey(v => v.ID);
            b.HasOne(v => v.Post)
                .WithMany(p => p.Views)
                .HasForeignKey(v => v.PostID)
                .OnDelete(DeleteBehavior.Cascade); // removing a post will remove its views
            b.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserID);
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });
        
        // ------ PostImage ------
        modelBuilder.Entity<PostImage>(b =>
        {
            b.HasKey(pi => pi.ID);
            b.HasOne(pi => pi.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PostID)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a post deletes its images
            b.Property(pi => pi.ImageFileID).IsRequired();
            b.Property(pi => pi.CreatedAt).HasDefaultValueSql("now()");
        });
        
        // ------ ConversationUser ------
        modelBuilder.Entity<ConversationUser>(b =>
        {
            b.HasKey(cu => new { cu.ConversationID, cu.UserID });

            b.HasOne(cu => cu.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(cu => cu.ConversationID)
                .OnDelete(DeleteBehavior.Cascade); // delete conversation -> delete participants

            b.HasOne(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserID)
                .OnDelete(DeleteBehavior.Cascade); // delete user -> remove from conversations

            b.Property(p => p.JoinedAt).HasDefaultValueSql("now()");
        });
        
        // ------ Conversation ------
        modelBuilder.Entity<Conversation>(b =>
        {
            b.HasKey(c => c.ID);
            b.Property(c => c.Name).IsRequired();
            b.Property(c => c.ProfilePicturePath).IsRequired();
            b.HasOne(c => c.Admin)
                .WithMany()
                .HasForeignKey(c => c.AdminID)
                .OnDelete(DeleteBehavior.SetNull);
            
            b.HasMany(c => c.Participants)
                .WithOne(p => p.Conversation)
                .HasForeignKey(p => p.ConversationID)
                .OnDelete(DeleteBehavior.Cascade); // removing a conversation removes its participants
            
            b.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationID)
                .OnDelete(DeleteBehavior.Cascade); // removing a conversation removes its messages
            
            b.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        });
        
        // ------ Message ------
        modelBuilder.Entity<Message>(b =>
        {
            b.HasKey(m => m.ID);
            b.Property(m => m.Content).IsRequired();
            b.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderID)
                .OnDelete(DeleteBehavior.Restrict); // sender can be deleted without affecting messages
            b.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationID)
                .OnDelete(DeleteBehavior.Cascade); // removing a conversation removes its messages
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });
        
        // ------ MessageImage ------
        modelBuilder.Entity<MessageImage>(b =>
        {
            b.HasKey(mi => mi.ID);
            b.HasOne(mi => mi.Message)
                .WithMany(m => m.Images)
                .HasForeignKey(mi => mi.MessageID)
                .OnDelete(DeleteBehavior.Cascade); // removing a message removes its images
            b.Property(mi => mi.ImageFileID).IsRequired();
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}