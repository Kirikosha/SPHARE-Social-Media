namespace Domain.Entities;
public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public int PublicationId { get; set; }
    public Publication Publication { get; set; } = null!;
    public DateTime CreationDate { get; set; }
}
