namespace Domain.Entities;

public class UserActionLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string? AdditionalDescription { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}