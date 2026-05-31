namespace Application.DTOs.UserDTOs;

public class UserUpdateDataDto
{
    public required string Username { get; set; }
    public required string UniqueNameIdentifier { get; set; }
    public string? Pronouns { get; set; }
    public string? ProfileDescription { get; set; }
    public List<string> Interests { get; set; } = [];
    public string? DateOfBirth { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageUrl { get; set; }
}