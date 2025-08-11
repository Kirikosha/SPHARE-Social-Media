namespace Domain.Entities;
public class Image
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? PublicId { get; set; }

    public int? PublicationId { get; set; }
    public Publication? Publication { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
}
