using Application.DTOs.CommentDTOs;

namespace Application.DTOs.ComplaintDTOs;
public class CommentComplaintDto
{
    public string Id { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string ComplainerId { get; set; } = string.Empty;
    public DateTime ComplainedAt { get; set; } = DateTime.UtcNow;
    public CommentDto Comment { get; set; } = null!;
}
