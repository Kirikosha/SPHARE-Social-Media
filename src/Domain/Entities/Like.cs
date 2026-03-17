namespace Domain.Entities;

public class Like
{
    public string PublicationId { get; set; } = string.Empty;
    public Publication Publication { get; set; } = null!;
    public string LikedById { get; set; } = string.Empty;
    public User LikedBy { get; set; } = null!;
}
