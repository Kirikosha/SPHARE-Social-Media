namespace Domain.Entities;
public class ErrorLog
{
    public int Id { get; set; }
    public required string ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }
}
