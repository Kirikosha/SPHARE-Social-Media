namespace Domain.DTOs.CommentDTOs;
public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
    public string PublicationId { get; set; } = string.Empty;
    public string? ParentCommentId { get; set; }
}
