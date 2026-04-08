namespace Domain.Entities;
public class Image
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ImageUrl { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;

    // Navigation properties
    public string? PublicationId { get; set; }
    public Publication? Publication { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
}
