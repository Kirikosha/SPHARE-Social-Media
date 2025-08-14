namespace Domain.DTOs.CommentDTOs;
public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
    public int PublicationId { get; set; }
}
