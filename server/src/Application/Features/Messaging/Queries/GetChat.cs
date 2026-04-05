using Application.Core;
using AutoMapper;
using Domain.DTOs.MessagingDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messaging.Queries;

public class GetChat
{
    public class Query : IRequest<Result<ChatWithMessagesDto>>
    {
        public required string ChatId { get; set; }
    }
    
    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, Result<ChatWithMessagesDto>>
    {
        public async Task<Result<ChatWithMessagesDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var chat = await context.Chats.Include(a => a.Participants).ThenInclude(a => a.User)
                .ThenInclude(a => a.ProfileImage)
                .Include(a => a.Messages.Take(50).OrderByDescending(c => c.SentAt))
                .FirstOrDefaultAsync(a => a.Id == request.ChatId, cancellationToken);
            

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
    }
}