namespace Domain.Entities.Complaints;
public class CommentComplaint : Complaint
{
    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
}
