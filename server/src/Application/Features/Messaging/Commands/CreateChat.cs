using Application.Core;
using Infrastructure;

namespace Application.Features.Messaging.Commands;

public class CreateChat
{
    public class Command : IRequest<Result<Chat>>
    {
        public required string User1Id { get; init; }
        public required string User2Id { get; init; }
    }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Result<Chat>>
    {
        public async Task<Result<Chat>> Handle(Command request, CancellationToken cancellationToken)
        {
            Chat chat = new Chat
            {
                Id = Guid.NewGuid().ToString(),
                Participants = new List<ChatUser>
                {
                    new() { UserId = request.User1Id },
                    new() { UserId = request.User2Id }
                }
            };

            await context.Chats.AddAsync(chat, cancellationToken);
            return Result<Chat>.Success(chat);
        }
    }
}