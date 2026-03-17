namespace Domain.Entities;
public class ChatUser
{
    public string ChatId { get; set; } = string.Empty;
    public Chat Chat { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
