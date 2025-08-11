namespace Domain.Entities;
using System.Collections.Generic;

public class Like
{
    public int PublicationId { get; set; }
    public Publication Publication { get; set; } = null!;
    public int LikedById { get; set; }
    public User LikedBy { get; set; } = null!;
    public List<Comment>? Comments { get; set; }
}
