using System.Runtime.CompilerServices;

namespace Domain.Entities;

public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Token { get; set; } = null!;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public bool IsRevoked { get; set; }
    
    // Navigation properties
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => !IsRevoked && !IsExpired;
}