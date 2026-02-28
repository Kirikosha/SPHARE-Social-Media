namespace Domain.DTOs.MessagingDTOs;

public class ChatUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsOnline { get; set; }
}