namespace Application.DTOs.UserDTOs;

public class PublicUserBriefDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool Blocked { get; set; }
}