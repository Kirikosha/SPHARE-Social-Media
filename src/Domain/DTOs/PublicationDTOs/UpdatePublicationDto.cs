namespace Domain.DTOs.PublicationDTOs;
public class UpdatePublicationDto
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public DateTime? RemindAt { get; set; }
}
