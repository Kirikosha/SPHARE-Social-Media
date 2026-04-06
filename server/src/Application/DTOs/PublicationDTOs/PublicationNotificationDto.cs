namespace Application.DTOs.PublicationDTOs;

public class PublicationNotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime PostedAt { get; set; }
    public bool WasSent { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string? AuthorImageUrl { get; set; }
    public string? FirstImageUrl { get; set; }
}