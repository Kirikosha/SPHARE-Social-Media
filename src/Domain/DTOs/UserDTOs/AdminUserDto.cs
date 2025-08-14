namespace Domain.DTOs.UserDTOs;
public class AdminUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ImageDto? ProfileImage { get; set; }
    public int ViolationScore { get; set; }
    public int AmountOfViolations { get; set; }
    public bool Blocked { get; set; }
    public DateTime? BlockedAt { get; set; }
}
