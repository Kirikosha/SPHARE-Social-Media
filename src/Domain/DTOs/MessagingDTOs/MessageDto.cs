namespace Domain.DTOs.MessagingDTOs;

public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool WasEdited { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SendersUsername { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}