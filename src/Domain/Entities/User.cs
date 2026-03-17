using Domain.Entities.Complaints;
using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Username { get; set; }
    public required string UniqueNameIdentifier { get; set; }
    public required string Email { get; set; }
    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];
    public Roles Role { get; set; } = Roles.User;
    public DateOnly DateOfCreation { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public List<Publication> CreatedPublications { get; set; } = [];
    public List<Like> LikedPublications { get; set; } = [];
    public string? ProfileImageId { get; set; }
    public Image? ProfileImage { get; set; }
    public int ViolationScore { get; set; }
    public List<Violation> Violations { get; set; } = [];
    public bool Blocked { get; set; }
    public DateTime? BlockedAt { get; set; }

    public UserProfileDetails? ProfileDetails { get; set; }
    public string? ProfileDetailsId { get; set; }

    public Address? Address { get; set; }
    public int? AddressId { get; set; }

    public List<PublicationComplaint> PublicationComplaintsMade { get; set; } = [];
    public List<CommentComplaint> CommentComplaintsMade { get; set; } = [];

    public List<ChatUser> Chats { get; set; } = [];
    
    public SpamRating SpamRating { get; set; } = null!;

    public List<UserActionLog> ActionLogs { get; set; } = [];

    public int SubscriberNumber { get; set; }
}
