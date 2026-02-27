using Application.Core;
using AutoMapper;
using Domain.DTOs.MessagingDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messaging.Queries;

public class GetUserChats
{
    public class Query : IRequest<Result<List<ChatDto>>>
    {
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, Result<List<ChatDto>>>
    {
        public async Task<Result<List<ChatDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var chats = await context.Chats
                .Include(c => c.Participants)
                .ThenInclude(c => c.User)
                .ThenInclude(c => c.ProfileImage)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.Participants.Any(p => p.UserId == request.UserId))
                .OrderByDescending(c => c.Messages.Max(m => m.SentAt))
                .ToListAsync(cancellationToken);

            var chatDtos = mapper.Map<List<ChatDto>>(chats);

            foreach (var chatDto in chatDtos)
            {
                var chat = chats.First(c => c.Id == chatDto.Id);

                chatDto.LastMessage = chat.Messages.Any() 
                    ? mapper.Map<MessageDto>(chat.Messages.OrderByDescending(m => m.SentAt).First()).Content! 
                    : null;

                chatDto.UnreadCount = await GetUnreadCount(chat.Id, request.UserId);
            }

            return Result<List<ChatDto>>.Success(chatDtos);
        }

        private async Task<int> GetUnreadCount(Guid chatId, int userId)
        {
            return await context.Messages
                .Where(m => m.ChatId == chatId
                            && m.SenderId != userId).CountAsync();
        }
    }
}