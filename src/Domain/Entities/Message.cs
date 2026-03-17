namespace Domain.Entities;
public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool WasEdited { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public User Sender { get; set; } = null!;

    public Guid ChatId { get; set; } = Guid.NewGuid();
    public Chat Chat { get; set; } = null!;
 } 