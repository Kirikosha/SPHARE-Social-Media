namespace Domain.DTOs.CommentDTOs;

using Domain.DTOs.UserDTOs;

public class CommentDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PublicUserDto Author { get; set; } = null!;
    public string PublicationId { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int RepliesAmount { get; set; }

}
