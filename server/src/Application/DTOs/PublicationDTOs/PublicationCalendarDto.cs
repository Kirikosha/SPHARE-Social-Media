namespace Application.DTOs.PublicationDTOs;

public class PublicationCalendarDto
{
    public string Id { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime? RemindAt { get; set; }
}