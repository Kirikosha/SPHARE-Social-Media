namespace Domain.DTOs.MessagingDTOs;

public class ChatDto
{
    public Guid Id { get; set; }
    public List<ChatUserDto> Participants { get; set; } = null!;
    public string LastMessage { get; set; } = string.Empty;
    public int UnreadCount { get; set; }

}