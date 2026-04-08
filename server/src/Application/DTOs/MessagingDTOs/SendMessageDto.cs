namespace Application.DTOs.MessagingDTOs;

public class SendMessageDto
{
    public string? ChatId { get; set; }
    public string ReceiverId { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
}