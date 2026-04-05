namespace Domain.DTOs.PublicationDTOs;
public class UpdatePublicationDto
{
    public string Id { get; set; }
    public string? Content { get; set; }
    public DateTime? RemindAt { get; set; }
    public int? ConditionTarget { get; set; }
}
