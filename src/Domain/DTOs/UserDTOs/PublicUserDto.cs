namespace Domain.DTOs.UserDTOs;
public class PublicUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string JoinedAt { get; set; }
    public ImageDto ProfileImage { get; set; }
    public bool Blocked { get; set; }
}
