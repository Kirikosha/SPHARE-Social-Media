namespace Domain.Entities;
using System;

public class Violation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ViolationText { get; set; } = "Some violation was made. Please contact the administration";
    public DateTime ViolatedAt { get; set; }
    
    // Navigation properties
    public string ViolatedById { get; set; } = string.Empty;
    public User ViolatedBy { get; set; } = null!;
}
