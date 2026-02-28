namespace Domain.DTOs.MessagingDTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool WasEdited { get; set; }
    public int SenderId { get; set; }
    public string SendersUsername { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}