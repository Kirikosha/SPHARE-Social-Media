using Application.Features.Messaging.Commands;
using Application.Helpers;
using Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.MessagingWebSockets;
public class ChatHub(IMediator mediator, ApplicationDbContext context) : Hub
{
    public async Task SendMessage(string? chatId, string receiverId, string message)
    {
        var userId = Context.User!.GetUserId();

        var savingResult = await mediator.Send(new SaveMessage.Command()
            { MessageContent = message, SenderId = userId, ReceiverId = receiverId, ChatId = chatId });

        if (savingResult.IsSuccess)
        {
            // Send to both participants
            await Clients.Group(savingResult.Value!.ChatId)
                .SendAsync("ReceiveMessage", savingResult.Value);
                
            // Also send a notification to the receiver if they're not connected
            await SendNotificationIfOffline(receiverId, userId, message);
        }
        else
        {
            await Clients.Caller.SendAsync("SendMessageError", savingResult.Error);
        }
    }
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.GetUserId();

        var userChatIds = await context.ChatUsers.Where(cu => cu.UserId == userId)
            .Select(cu => cu.ChatId.ToString())
            .ToListAsync();

        foreach (var chatId in userChatIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        await NotifyContacts(userId, true);

        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.GetUserId();
        
        // Get all chats the user is part of
        var userChatIds = await context.ChatUsers
            .Where(cu => cu.UserId == userId)
            .Select(cu => cu.ChatId.ToString())
            .ToListAsync();
        
        // Remove connection from all chat groups
        foreach (var chatId in userChatIds)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }
        
        // Notify user's contacts that they're offline
        await NotifyContacts(userId, false);
        
        await base.OnDisconnectedAsync(exception);
    }

    private async Task NotifyContacts(string userId, bool isOnline)
    {
        var contactIds = await context.ChatUsers
            .Where(cu => cu.UserId == userId)
            .SelectMany(cu => context.ChatUsers
                .Where(cu2 => cu2.ChatId == cu.ChatId && cu2.UserId != userId)
                .Select(cu2 => cu2.UserId))
            .Distinct()
            .ToListAsync();

        foreach (var contactId in contactIds)
        {
            await Clients.User(contactId).SendAsync("ContactPresenceChanged", userId, isOnline);
        }
    }

    private async Task SendNotificationIfOffline(string receiverId, string senderId, string content)
    {
        // TODO: Make logic here about situation when the user is not online yet the message should
        // be delivered. Consider using SSE (Server Sent Event) here
    }
    

    public async Task JoinChat(string chatId)
    {
        var userId = Context.User.GetUserId();

        var isparticipant = await context.ChatUsers
            .AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);

        if (!isparticipant)
        {
            throw new HubException("Not authorized to join this chat");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);

        await Clients.Group(chatId)
            .SendAsync("UserJoinedChat", userId);
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);

        var userId = Context.User.GetUserId();
        await Clients.Group(chatId).SendAsync("UserLeftChat", userId);
    }

    public async Task SendTypingIndicator(string chatId)
    {
        var userId = Context.User!.GetUserId();
        await Clients.Group(chatId)
            .SendAsync("UserTyping", userId, chatId);
    }
    
    public async Task StopTypingIndicator(string chatId)
    {
        var userId = Context.User!.GetUserId();
        await Clients.Group(chatId)
            .SendAsync("UserStoppedTyping", userId, chatId);
    }
}
