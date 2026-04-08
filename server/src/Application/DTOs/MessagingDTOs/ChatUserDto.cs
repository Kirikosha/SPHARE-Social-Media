namespace Application.DTOs.MessagingDTOs;

public class ChatUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsOnline { get; set; }
}