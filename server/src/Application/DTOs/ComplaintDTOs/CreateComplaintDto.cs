namespace Application.DTOs.ComplaintDTOs;
public class CreateComplaintDto
{
    public required string Reason { get; set; }
    public string? Explanation { get; set; }
    public string TargetId { get; set; } = string.Empty;
}
