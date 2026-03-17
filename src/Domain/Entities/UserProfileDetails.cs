namespace Domain.Entities;
public class UserProfileDetails
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Pronouns { get; set; }
    public string? MainProfileDescription { get; set; } // For example like "I am blah blah blah, working there"
    public List<string> Interests { get; set; } = [];
    public DateTime? DateOfBirth { get; set; }

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
