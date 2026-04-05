namespace Domain.Entities.Complaints;
public class CommentComplaint : Complaint
{
    public string CommentId { get; set; } = string.Empty;
    public Comment Comment { get; set; } = null!;
}
