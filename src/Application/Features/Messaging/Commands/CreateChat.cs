using Application.Core;
using Infrastructure;

namespace Application.Features.Messaging.Command;

public class CreateChat
{
    public class Command : IRequest<Result<Chat>>
    {
        public required int User1Id { get; set; }
        public required int User2Id { get; set; }
    }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Result<Chat>>
    {
        public async Task<Result<Chat>> Handle(Command request, CancellationToken cancellationToken)
        {
            Chat chat = new Chat
            {
                Id = Guid.NewGuid(),
                Participants = new List<ChatUser>
                {
                    new ChatUser { UserId = request.User1Id },
                    new ChatUser { UserId = request.User2Id }
                }
            };

            await context.Chats.AddAsync(chat, cancellationToken);
            return Result<Chat>.Success(chat);
        }
    }
}