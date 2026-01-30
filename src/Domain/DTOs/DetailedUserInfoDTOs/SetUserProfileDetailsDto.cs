namespace Domain.DTOs.DetailedUserInfoDTOs;
public class SetUserProfileDetailsDto
{
    public string? Pronouns { get; set; }
    public string? MainProfileDescription { get; set; }
    public List<string> Interests { get; set; } = [];
    public DateTime? DateOfBirth { get; set; }
}
