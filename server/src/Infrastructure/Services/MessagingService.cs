using Application.Core;
using Application.DTOs.MessagingDTOs;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class MessagingService(ApplicationDbContext context, IMapper mapper) : IMessagingService
{
    private IMessagingService _iMessagingServiceImplementation;

    public async Task<Result<ChatWithMessagesDto>> GetChatAsync(string chatId, CancellationToken ct)
    {
        var chat = await context.Chats.Include(a => a.Participants).ThenInclude(a => a.User)
            .ThenInclude(a => a.ProfileImage)
            .Include(a => a.Messages.Take(50).OrderByDescending(c => c.SentAt))
            .FirstOrDefaultAsync(a => a.Id == chatId, ct);
            

        if (chat == null) return Result<ChatWithMessagesDto>.Failure("Chat does not exist", 400);

        var participants = mapper.Map<List<ChatUserDto>>(chat.Participants);
        var messages = mapper.Map<List<MessageDto>>(chat.Messages);
        var mappedChat = new ChatWithMessagesDto()
        {
            Id = chat.Id,
            // TODO: for now unread count is 0 and will be changed when the IsRead property is applied to the db
            UnreadCount = 0,
            Participants = participants,
            Messages = messages,
        };

        if (messages.Count == 0)
        {
            mappedChat.LastMessage = string.Empty;
        }
        else
        {
            mappedChat.LastMessage = messages[0].Content;
        }

        return Result<ChatWithMessagesDto>.Success(mappedChat);
    }

    public async Task<Result<List<ChatDto>>> GetUserChatsAsync(string userId, CancellationToken ct)
    {
        var chats = await context.Chats
            .Include(c => c.Participants)
            .ThenInclude(c => c.User)
            .ThenInclude(c => c.ProfileImage)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.Messages.Max(m => m.SentAt))
            .ToListAsync(ct);

        var chatDtos = mapper.Map<List<ChatDto>>(chats);

        foreach (var chatDto in chatDtos)
        {
            var chat = chats.First(c => c.Id == chatDto.Id);

            chatDto.LastMessage = chat.Messages.Count != 0
                ? mapper.Map<MessageDto>(chat.Messages.OrderByDescending(m => m.SentAt).First()).Content 
                : null;

            chatDto.UnreadCount = await GetUnreadCount(chat.Id, userId);
        }

        return Result<List<ChatDto>>.Success(chatDtos);
    }

    public async Task<Result<List<MessageDto>>> GetMessagesBatchAsync(string chatId, string userId, int page, int 
            pageSize, CancellationToken ct)
    {
        var isParticipant = await context.ChatUsers
            .AnyAsync(cu => cu.ChatId == chatId &&
                            cu.UserId == userId, ct);

        if (!isParticipant)
        {
            return Result<List<MessageDto>>.Failure("You are not a part of this conversation!", 403);
        }

        var messages = await context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var messageDtoList = mapper.Map<List<MessageDto>>(messages);

        return Result<List<MessageDto>>.Success(messageDtoList);
    }

    public async Task<Result<ChatDto>> StartChatAsync(StartChatDto startDto, string userId, CancellationToken ct)
    {
        if (userId == startDto.OtherUserId)
            return Result<ChatDto>.Failure("Can't start chat with yourself!", 400);

        var existingChat = await context.Chats
            .Include(a => a.Participants)
            .Where(c => c.Participants.Any(p => p.UserId == userId) &&
                        c.Participants.Any(p => p.UserId == startDto.OtherUserId))
            .FirstOrDefaultAsync(ct);

        if (existingChat != null)
        {
            return Result<ChatDto>.Success(mapper.Map<ChatDto>(existingChat));
        }

        var chat = new Chat()
        {
            Id = Guid.NewGuid().ToString(),
            Participants = new List<ChatUser>
            {
                new() { UserId = userId },
                new() { UserId = startDto.OtherUserId }
            }
        };

        context.Chats.Add(chat);
        await context.Entry(chat)
            .Collection(c => c.Participants)
            .Query()
            .Include(p => p.User)
            .LoadAsync(ct);

        var chatDto = mapper.Map<ChatDto>(chat);

        return Result<ChatDto>.Success(chatDto);
    }

    public async Task<Result<ReceiveMessageDto>> SaveMessageAsync(string? chatId, string receiverId, string senderId, 
        string message, CancellationToken ct)
    {
        var sender = await context.Users.FindAsync(senderId, ct);
        if (sender == null)
            return Result<ReceiveMessageDto>.Failure("Sender was not found", 404);
            
        var receiver = await context.Users.FindAsync(receiverId, ct);
        if (receiver == null)
            return Result<ReceiveMessageDto>.Failure( "Receiver was not found", 404);
            
            
                
        // means that there is no chat yet created
        Chat? chat;
        if (string.IsNullOrEmpty(chatId))
        {
            // have to create chat
            chat = CreateChat(receiverId, senderId);
        }
        else
        {
            chat = await context.Chats.FirstOrDefaultAsync(a => a.Id == chatId, ct);
            if (chat == null)
                chat = CreateChat(receiverId, senderId);
        }

        var newMessage = new Message
        {
            Id = Guid.NewGuid().ToString(),
            ChatId = chat.Id,
            SenderId = senderId,
            Content = message,
            SentAt = DateTime.UtcNow
        };

        context.Messages.Add(newMessage);
            
        var messageDto = new ReceiveMessageDto
        {
            ChatId = chat.Id,
            MessageId = newMessage.Id,
            Content = newMessage.Content,
            SentAt = newMessage.SentAt,
            SenderId = newMessage.SenderId,
            SenderUsername = sender.Username
        };
    
        return Result<ReceiveMessageDto>.Success(messageDto);
    }

    public async Task<Result<Chat>> CreateChatAsync(string userId1, string userId2, CancellationToken ct)
    {
        Chat chat = new Chat
        {
            Id = Guid.NewGuid().ToString(),
            Participants = new List<ChatUser>
            {
                new() { UserId = userId1},
                new() { UserId = userId2}
            }
        };

        await context.Chats.AddAsync(chat, ct);
        return Result<Chat>.Success(chat);
    }

    private async Task<int> GetUnreadCount(string chatId, string userId)
    {
        return await context.Messages
            .Where(m => m.ChatId == chatId
                        && m.SenderId != userId).CountAsync();
    }
    
    private Chat CreateChat(string receiverId, string senderId)
    {
        string chatId = Guid.NewGuid().ToString();

        Chat chat = new Chat()
        {
            Id = chatId
        };

        context.Chats.Add(chat);
                
        context.ChatUsers.AddRange(
            new ChatUser
            {
                ChatId = chatId,
                UserId = senderId
            },
            new ChatUser
            {
                ChatId = chatId,
                UserId = receiverId
            }
        );

        return chat;
    }
}