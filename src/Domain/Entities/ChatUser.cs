namespace Domain.Entities;
public class ChatUser
{
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
