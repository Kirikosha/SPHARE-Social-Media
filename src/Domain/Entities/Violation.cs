namespace Domain.Entities;
using System;

public class Violation
{
    public int Id { get; set; }
    public int ViolatedById { get; set; }
    public User ViolatedBy { get; set; }
    public string ViolationText { get; set; }
    public DateTime ViolatedAt { get; set; }
}
