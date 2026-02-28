namespace Domain.DTOs.PublicationDTOs;

public class PublicationCalendarDto
{
    public int Id { get; set; } 
    public string? Content { get; set; }
    public DateTime? RemindAt { get; set; }
}