namespace Domain.Entities.RecomendationEntities;

public class UserInterestTag
{
    public float Weight { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}