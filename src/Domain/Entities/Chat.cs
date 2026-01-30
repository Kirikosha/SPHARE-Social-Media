namespace Domain.Entities;
public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<ChatUser> Participants { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
}
