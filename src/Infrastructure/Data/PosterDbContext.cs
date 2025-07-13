using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PosterDbContext : DbContext
{
    public PosterDbContext(DbContextOptions<PosterDbContext> options) : base(options)
    { }

    public DbSet<User> Users => Set<User>();
    //public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.ID);

            user.OwnsOne(u => u.Username, username =>
            {
                username.Property(un => un.Value)
                    .HasColumnName("Username")
                    .IsRequired();
            });

            user.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired();
            });

            user.Property(u => u.PasswordHash)
                .HasMaxLength(256);

            /*user.HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);*/
        });

        /*modelBuilder.Entity<Post>(post =>
        {
            post.HasKey(p => p.ID);
            post.Property(p => p.Content).IsRequired().HasMaxLength(280);
            post.Property(p => p.CreatedAt).IsRequired();
            post.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });*/
    }
}