namespace Application.DTOs.PublicationDTOs;

public class UpdatePublicationContentDto
{
    public required string PublicationId { get; set; }
    public string? NewContent { get; set; }
}