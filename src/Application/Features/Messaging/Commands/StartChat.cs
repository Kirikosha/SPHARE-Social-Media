using Application.Core;
using AutoMapper;
using Domain.DTOs.MessagingDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messaging.Command;

public class StartChat
{
    public class Command : IRequest<Result<ChatDto>>
    {
        public required StartChatDto ChatRequest { get; set; }
        public required int UserId { get; set; }
    }
    
    public class Handler(ApplicationDbContext context, IMapper mapper)
        : IRequestHandler<Command, Result<ChatDto>>
    {
        public async Task<Result<ChatDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.UserId == request.ChatRequest.OtherUserId)
                return Result<ChatDto>.Failure("Can't start chat with yourself!", 400);

            var existingChat = await context.Chats
                .Include(a => a.Participants)
                .Where(c => c.Participants.Any(p => p.UserId == request.UserId) &&
                            c.Participants.Any(p => p.UserId == request.ChatRequest.OtherUserId))
                .FirstOrDefaultAsync(cancellationToken);

            if (existingChat != null)
            {
                return Result<ChatDto>.Success(mapper.Map<ChatDto>(existingChat));
            }

            var chat = new Chat()
            {
                Id = Guid.NewGuid(),
                Participants = new List<ChatUser>
                {
                    new ChatUser() { UserId = request.UserId },
                    new ChatUser() { UserId = request.ChatRequest.OtherUserId }
                }
            };

            context.Chats.Add(chat);
            await context.Entry(chat)
                .Collection(c => c.Participants)
                .Query()
                .Include(p => p.User)
                .LoadAsync(cancellationToken);

            var chatDto = mapper.Map<ChatDto>(chat);

            return Result<ChatDto>.Success(chatDto);
        }
    }
}