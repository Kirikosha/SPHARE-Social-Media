using Application.Core;
using Domain.DTOs.MessagingDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messaging.Commands;

public class SaveMessage
{
    public class Command : IRequest<Result<ReceiveMessageDto>>
    {
        public string? ChatId { get; set; }
        public required string ReceiverId { get; set; }
        public required string SenderId { get; set; }
        public required string MessageContent { get; set; }
    }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Result<ReceiveMessageDto>>
    {
        public async Task<Result<ReceiveMessageDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var sender = await context.Users.FindAsync(request.SenderId, cancellationToken);
            if (sender == null)
                return Result<ReceiveMessageDto>.Failure("Sender was not found", 404);
            
            var receiver = await context.Users.FindAsync(request.ReceiverId, cancellationToken);
            if (receiver == null)
                return Result<ReceiveMessageDto>.Failure( "Receiver was not found", 404);
            
            
                
            // means that there is no chat yet created
            Chat? chat;
            if (string.IsNullOrEmpty(request.ChatId))
            {
                // have to create chat
                chat = CreateChat(request.ReceiverId, request.SenderId);
            }
            else
            {
                chat = await context.Chats.FirstOrDefaultAsync(a => a.Id == request.ChatId, cancellationToken);
                if (chat == null)
                    chat = CreateChat(request.ReceiverId, request.SenderId);
            }

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                ChatId = chat.Id,
                SenderId = request.SenderId,
                Content = request.MessageContent,
                SentAt = DateTime.UtcNow
            };

            context.Messages.Add(message);
            
            var messageDto = new ReceiveMessageDto
            {
                ChatId = chat.Id,
                MessageId = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                SenderId = message.SenderId,
                SenderUsername = sender.Username
            };
    
            return Result<ReceiveMessageDto>.Success(messageDto);
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
}