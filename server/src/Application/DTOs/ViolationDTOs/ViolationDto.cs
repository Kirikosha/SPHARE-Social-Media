namespace Application.DTOs.ViolationDTOs;
public class ViolationDto
{
    public string Id { get; set; } = string.Empty;
    public string ViolationText { get; set; } = string.Empty;
    public DateTime ViolatedAt { get; set; }
}
