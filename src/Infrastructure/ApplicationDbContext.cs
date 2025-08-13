namespace Infrastructure;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Publication> Publications { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Violation> Violations { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ErrorLog>().ToTable("ErrorLogs", t => t.ExcludeFromMigrations());

        modelBuilder.Entity<Like>()
            .HasKey(k => new { k.LikedById, k.PublicationId });

        modelBuilder.Entity<Like>()
            .HasOne(l => l.LikedBy)
            .WithMany(u => u.LikedPublications)
            .HasForeignKey(l => l.LikedById)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Publication)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.CreatedPublications)
            .WithOne(p => p.Author)
            .HasForeignKey(l => l.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasOne(i => i.User)
            .WithOne(a => a.ProfileImage)
            .HasForeignKey<User>(u => u.ProfileImageId)
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Publication)
            .WithMany(a => a.Images)
            .HasForeignKey(a => a.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
