namespace Application.DTOs.MessagingDTOs;

public class ChatWithMessagesDto : ChatDto
{
    public List<MessageDto> Messages { get; set; } = [];
}