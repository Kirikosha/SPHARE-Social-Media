using System.Text.RegularExpressions;
using Application.Features.Messaging.Command;
using Application.Helpers;
using Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Tls;

namespace Application.MessagingWebSockets;
public class ChatHub(IMediator mediator, ApplicationDbContext context) : Hub
{
    public async Task SendMessage(Guid? chatId, int receiverId, string message)
    {
        var userId = Context.User!.GetUserId();

        var savingResult = await mediator.Send(new SaveMessage.Command()
            { MessageContent = message, SenderId = userId, ReceiverId = receiverId, ChatId = chatId });

        if (savingResult.IsSuccess)
        {
            // Send to both participants
            await Clients.Group(savingResult.Value.ChatId.ToString())
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

    private async Task NotifyContacts(int userId, bool isOnline)
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
            await Clients.User(contactId.ToString()).SendAsync("ContactPresenceChanged", userId, isOnline);
        }
    }

    private async Task SendNotificationIfOffline(int receiverId, int senderId, string content)
    {
        // TODO: Make logic here about situation when the user is not online yet the message should
        // be delivered. Consider using SSE (Server Sent Event) here
    }
    

    public async Task JoinChat(Guid chatId)
    {
        var userId = Context.User.GetUserId();

        var isparticipant = await context.ChatUsers
            .AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);

        if (!isparticipant)
        {
            throw new HubException("Not authorized to join this chat");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

        await Clients.Group(chatId.ToString())
            .SendAsync("UserJoinedChat", userId);
    }

    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());

        var userId = Context.User.GetUserId();
        await Clients.Group(chatId.ToString()).SendAsync("UserLeftChat", userId);
    }

    public async Task SendTypingIndicator(Guid chatId)
    {
        var userId = Context.User!.GetUserId();
        await Clients.Group(chatId.ToString())
            .SendAsync("UserTyping", userId, chatId);
    }
    
    public async Task StopTypingIndicator(Guid chatId)
    {
        var userId = Context.User!.GetUserId();
        await Clients.Group(chatId.ToString())
            .SendAsync("UserStoppedTyping", userId, chatId);
    }
}
