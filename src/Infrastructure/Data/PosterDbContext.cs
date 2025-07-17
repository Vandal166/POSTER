using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PosterDbContext : DbContext
{
    public PosterDbContext(DbContextOptions<PosterDbContext> options) : base(options)
    { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<PostView> PostViews => Set<PostView>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        /*
         *  Entity 'User' has a global query filter defined and is the required end of a relationship with the entity 'Post'. This may lead to unexpected results when the required entity is filtered out. Either configure the na
vigation as optional, or define matching query filters for both entities in the navigation.
etc
         */
        
        // ------ User ------
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.ID);
            b.OwnsOne(u => u.Username, x => x.Property(p => p.Value).HasColumnName("Username").IsRequired());
            b.OwnsOne(u => u.Email,    x => x.Property(p => p.Value).HasColumnName("Email").IsRequired());
            b.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
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
        });
       
        // ------ PostLike ------
        modelBuilder.Entity<PostLike>(b =>
        {
            b.HasKey(pl => pl.ID);
            b.HasOne(pl => pl.Post)
                .WithMany()
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
                .WithMany()
                .HasForeignKey(v => v.PostID)
                .OnDelete(DeleteBehavior.Cascade); // removing a post will remove its views
            b.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserID);
            b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}