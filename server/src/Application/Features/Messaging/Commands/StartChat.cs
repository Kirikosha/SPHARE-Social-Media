namespace Application.Features.Messaging.Commands;
using Core;
using DTOs.MessagingDTOs;
using Application.Interfaces.Services;

public class StartChat
{
    public class Command : IRequest<Result<ChatDto>>
    {
        public required StartChatDto ChatRequest { get; set; }
        public required string UserId { get; set; }
    }
    
    public class Handler(IMessagingService messagingService)
        : IRequestHandler<Command, Result<ChatDto>>
    {
        public async Task<Result<ChatDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await messagingService.StartChatAsync(request.ChatRequest, request.UserId, cancellationToken);
        }
    }
}