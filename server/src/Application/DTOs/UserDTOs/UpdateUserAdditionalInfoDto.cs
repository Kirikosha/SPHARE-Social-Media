namespace Application.DTOs.UserDTOs;

public class UpdateUserAdditionalInfoDto
{
    public string? Pronouns { get; set; }
    public string? MainProfileDescription { get; set; }
    public List<string> Interests { get; set; } = [];
    public DateTime? DateOfBirth { get; set; }
}