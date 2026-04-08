using Application.Core;
using Application.DTOs.MessagingDTOs;
using Application.Interfaces.Services;

namespace Application.Features.Messaging.Queries;

public class GetUserChats
{
    public class Query : IRequest<Result<List<ChatDto>>>
    {
        public required string UserId { get; set; }
    }

    public class Handler(IMessagingService messagingService) : IRequestHandler<Query, Result<List<ChatDto>>>
    {
        public async Task<Result<List<ChatDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await messagingService.GetUserChatsAsync(request.UserId, cancellationToken);
        }
    }
}