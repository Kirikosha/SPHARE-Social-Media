namespace Application.Features.Messaging.Commands;
using Core;
using DTOs.MessagingDTOs;
using Application.Interfaces.Services;

public class SaveMessage
{
    public class Command : IRequest<Result<ReceiveMessageDto>>
    {
        public string? ChatId { get; set; }
        public required string ReceiverId { get; set; }
        public required string SenderId { get; set; }
        public required string MessageContent { get; set; }
    }
    
    public class Handler(IMessagingService messagingService) : IRequestHandler<Command, Result<ReceiveMessageDto>>
    {
        public async Task<Result<ReceiveMessageDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await messagingService.SaveMessageAsync(request.ChatId, request.ReceiverId, request.SenderId,
                request.MessageContent, cancellationToken);
        }
    }
}