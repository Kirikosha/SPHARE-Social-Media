namespace Infrastructure;

using Domain.Entities;
using Domain.Entities.Complaints;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Publication> Publications { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Violation> Violations { get; set; }
    public DbSet<UserProfileDetails> ProfileDetails { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<PublicationComplaint> PublicationComplaints { get; set; }
    public DbSet<CommentComplaint> CommentComplaints { get; set; }
    public DbSet<CommentClosure> CommentTrees { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ChatUser> ChatUsers { get; set; }
    public DbSet<SpamRating> SpamRatings { get; set; }
    public DbSet<UserActionLog> UserLogs { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
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

        modelBuilder.Entity<User>()
            .HasOne(u => u.Address)
            .WithOne(a => a.User)
            .HasForeignKey<Address>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.ProfileDetails)
            .WithOne(d => d.User)
            .HasForeignKey<UserProfileDetails>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // complaints 
        modelBuilder.Entity<PublicationComplaint>()
            .HasOne(c => c.Publication)
            .WithMany(a => a.PublicationComplaints)
            .HasForeignKey(c => c.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentComplaint>()
            .HasOne(c => c.Comment)
            .WithMany(a => a.CommentComplaints)
            .HasForeignKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // comment trees
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CommentClosure>()
            .HasKey(x => new { x.AncestorId, x.DescendantId });

        modelBuilder.Entity<CommentClosure>()
            .HasOne(x => x.Ancestor)
            .WithMany(c => c.Descendants)
            .HasForeignKey(x => x.AncestorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentClosure>()
            .HasOne(x => x.Descendant)
            .WithMany(c => c.Ancestors)
            .HasForeignKey(x => x.DescendantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentClosure>()
            .HasIndex(x => new { x.AncestorId, x.Depth });


        modelBuilder.Entity<CommentClosure>()
            .HasIndex(x => new { x.DescendantId, x.Depth });

        modelBuilder.Entity<Comment>()
            .HasIndex(c => new { c.PublicationId, c.ParentCommentId, c.CreationDate });


        modelBuilder.Entity<UserProfileDetails>()
            .Property(x => x.DateOfBirth)
            .HasColumnType("date");

        modelBuilder.Entity<ChatUser>()
            .HasKey(x => new { x.ChatId, x.UserId });

        modelBuilder.Entity<ChatUser>()
            .HasOne(x => x.Chat)
            .WithMany(c => c.Participants)
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatUser>()
            .HasOne(x => x.User)
            .WithMany(c => c.Chats)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SpamRating>()
            .HasOne(u => u.User)
            .WithOne(s => s.SpamRating)
            .HasForeignKey<SpamRating>(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade); 

        modelBuilder.Entity<User>()
            .HasMany(c => c.ActionLogs)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
