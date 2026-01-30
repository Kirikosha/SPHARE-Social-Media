namespace Domain.DTOs.CommentDTOs;

using Domain.DTOs.UserDTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public PublicUserDto Author { get; set; } = null!;
    public int PublicationId { get; set; }
    public DateTime CreationDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int RepliesAmount { get; set; }

}
