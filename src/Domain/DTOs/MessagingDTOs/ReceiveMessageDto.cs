namespace Domain.DTOs.MessagingDTOs;

public class ReceiveMessageDto
{
    public Guid ChatId { get; set; }
    public Guid MessageId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
}