namespace Application.DTOs.PublicationDTOs;

public class UpdatePlannedPublicationDto
{
    public required string PublicationId { get; set; }
    public required DateTime PublishAt { get; set; }
}