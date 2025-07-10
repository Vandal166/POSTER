using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PosterDbContext : DbContext
{
    public PosterDbContext(DbContextOptions<PosterDbContext> options) : base(options)
    { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.ID);
            user.HasIndex(u => u.Username).IsUnique();
            user.Property(u => u.Username).IsRequired().HasMaxLength(50);
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.Email).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Post>(post =>
        {
            post.HasKey(p => p.ID);
            post.Property(p => p.Content).IsRequired().HasMaxLength(280);
            post.Property(p => p.CreatedAt).IsRequired();
            post.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}