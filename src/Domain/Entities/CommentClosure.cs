namespace Domain.Entities;
public class CommentClosure
{
    public int AncestorId { get; set; }
    public Comment Ancestor { get; set; }

    public int DescendantId { get; set; }
    public Comment Descendant { get; set; } = null!;
    
    public int Depth { get; set; } // 0 - self, 1 - parent, 2 - grandparent ....
}
