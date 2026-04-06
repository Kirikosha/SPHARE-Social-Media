namespace Application.DTOs.DetailedUserInfoDTOs;
public class UserProfileDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string? Pronouns { get; set; }
    public string? MainProfileDescription { get; set; }
    public List<string> Interests { get; set; } = [];
    public DateTime? DateOfBirth { get; set; }
}
