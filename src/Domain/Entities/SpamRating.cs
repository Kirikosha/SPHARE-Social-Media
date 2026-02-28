namespace Domain.Entities;

public class SpamRating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public double SpamValue { get; set; } = 0.0;
}