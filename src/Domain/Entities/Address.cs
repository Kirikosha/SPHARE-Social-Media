namespace Domain.Entities;
public class Address
{
    public int Id { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
