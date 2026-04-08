namespace Application.Features.Messaging.Queries;
using Core;
using DTOs.MessagingDTOs;
using Application.Interfaces.Services;
public class GetChat
{
    public class Query : IRequest<Result<ChatWithMessagesDto>>
    {
        public required string ChatId { get; set; }
    }
    
    public class Handler(IMessagingService messagingService) : IRequestHandler<Query, Result<ChatWithMessagesDto>>
    {
        public async Task<Result<ChatWithMessagesDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await messagingService.GetChatAsync(request.ChatId, cancellationToken);
        }
    }
}