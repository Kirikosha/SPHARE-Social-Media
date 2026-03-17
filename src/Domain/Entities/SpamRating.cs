namespace Domain.Entities;

public class SpamRating
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public double SpamValue { get; set; }
}