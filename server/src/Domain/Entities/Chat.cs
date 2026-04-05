namespace Domain.Entities;
public class Chat
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<ChatUser> Participants { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
}
