namespace Domain.DTOs.MessagingDTOs;

public class SendMessageDto
{
    public Guid? ChatId { get; set; }
    public int ReceiverId { get; set; }
    public string MessageContent { get; set; } = string.Empty;
}