using Domain.DTOs.PublicationDTOs;
using Domain.Entities;

namespace Domain.DTOs.ComplaintDTOs;
public class PublicationComplaintDto
{
    public int Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int ComplainerId { get; set; }
    public DateTime ComplainedAt { get; set; } = DateTime.UtcNow;
    public PublicationDto Publication { get; set; } = null!;
}
