using Domain.Entities.Complaints;

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
    public bool IsDeleted { get; set; } = false;
    // Complaints
    public List<CommentComplaint> CommentComplaints { get; set; } = [];

    // Comment tree impl
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public List<Comment> Replies { get; set; } = [];

    public List<CommentClosure> Ancestors { get; set; } = [];
    public List<CommentClosure> Descendants { get; set; } = [];
}
