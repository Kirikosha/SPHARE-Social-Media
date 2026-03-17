namespace Domain.Entities.Complaints;
public abstract class Complaint
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Reason { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string ComplainerId { get; set; } = string.Empty;
    public User Complainer { get; set; } = null!;
    public DateTime ComplainedAt { get; set; } = DateTime.UtcNow;
    public double ComplaintValue { get; set; }
}
