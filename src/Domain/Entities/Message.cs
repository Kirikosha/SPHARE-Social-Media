namespace Domain.Entities;
public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool WasEdited { get; set; } = false;
    public User SentBy { get; set; } = null!;

    public Guid ChatId { get; set; } = Guid.NewGuid();
    public Chat Chat { get; set; } = null!;
 } 