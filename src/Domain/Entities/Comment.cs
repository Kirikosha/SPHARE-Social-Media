using Domain.Entities.Complaints;

namespace Domain.Entities;
public class Comment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public User Author { get; set; } = null!;
    public Publication Publication { get; set; } = null!;
    public string AuthorId { get; set; }
    public string PublicationId { get; set; }
    public List<CommentComplaint> CommentComplaints { get; set; } = [];
    
    // Comment tree impl
    public string? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public List<Comment> Replies { get; set; } = [];

    public List<CommentClosure> Ancestors { get; set; } = [];
    public List<CommentClosure> Descendants { get; set; } = [];
}
