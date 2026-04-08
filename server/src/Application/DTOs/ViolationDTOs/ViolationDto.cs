namespace Application.DTOs.ViolationDTOs;
public class ViolationDto
{
    public int Id { get; set; }
    public string ViolationText { get; set; } = string.Empty;
    public DateTime ViolatedAt { get; set; }
}
