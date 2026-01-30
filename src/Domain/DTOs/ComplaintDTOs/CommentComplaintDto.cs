using Domain.DTOs.CommentDTOs;
using Domain.DTOs.PublicationDTOs;

namespace Domain.DTOs.ComplaintDTOs;
public class CommentComplaintDto
{
    public int Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int ComplainerId { get; set; }
    public DateTime ComplainedAt { get; set; } = DateTime.UtcNow;
    public CommentDto Comment { get; set; } = null!;
}
