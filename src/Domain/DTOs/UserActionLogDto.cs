namespace Domain.DTOs;

public class UserActionLogDto
{
    public required int UserId { get; set; }
    public string? AdditionalDescription { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}