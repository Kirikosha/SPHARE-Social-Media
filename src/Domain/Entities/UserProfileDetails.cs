namespace Domain.Entities;
public class UserProfileDetails
{
    public int Id { get; set; }
    public string? Pronouns { get; set; }
    public string? MainProfileDescription { get; set; } // For example like "I am blah blah blah, working there"
    public List<string> Interests { get; set; } = [];
    public DateTime? DateOfBirth { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
