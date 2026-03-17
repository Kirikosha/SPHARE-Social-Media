namespace Domain.DTOs.MessagingDTOs;

public class ChatDto
{
    public string Id { get; set; } = string.Empty;
    public List<ChatUserDto> Participants { get; set; } = null!;
    public string LastMessage { get; set; } = string.Empty;
    public int UnreadCount { get; set; }

}