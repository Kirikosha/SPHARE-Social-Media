namespace Domain.Entities;

public class UserActionLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Action { get; set; } = string.Empty;
    public string? AdditionalDescription { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string? TargetId { get; set; }
    
    // Navigation properties
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}