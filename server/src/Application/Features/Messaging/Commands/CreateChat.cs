namespace Application.Features.Messaging.Commands;
using Core;
using Application.Interfaces.Services;

public class CreateChat
{
    public class Command : IRequest<Result<Chat>>
    {
        public required string User1Id { get; init; }
        public required string User2Id { get; init; }
    }
    
    public class Handler(IMessagingService messagingService) : IRequestHandler<Command, Result<Chat>>
    {
        public async Task<Result<Chat>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await messagingService.CreateChatAsync(request.User1Id, request.User1Id, cancellationToken);
        }
    }
}