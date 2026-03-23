namespace Domain.Entities;

public class PublicationView
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    public int DwellSeconds { get; set; }
    
    // Navigation properties
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public string PublicationId { get; set; } = string.Empty;
    public Publication Publication { get; set; } = null!;
}