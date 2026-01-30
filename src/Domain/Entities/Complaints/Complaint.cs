namespace Domain.Entities.Complaints;
public abstract class Complaint
{
    public int Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int ComplainerId { get; set; }
    public User Complainer { get; set; } = null!;
    public DateTime ComplainedAt { get; set; } = DateTime.UtcNow;
}
