namespace Domain.Entities;
public class Address
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? City { get; set; }
    public string? Country { get; set; }

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = default!;
}
