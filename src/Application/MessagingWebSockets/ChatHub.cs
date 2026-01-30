using Application.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace Application.MessagingWebSockets;
public class ChatHub(IMediator mediator) : Hub
{
    // TODO: Finish the method
    public async Task SendMessage(Guid chatId, string message)
    {
        var userId = Context.User!.GetUserId();

        await Clients.Group(chatId.ToString())
            .SendAsync("ReceiveMessage", chatId, message);
    }
    public async override Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            chatId.ToString()
        );
    }

    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            chatId.ToString()
        );
    }

}
