namespace Application.Features.Messaging.Queries;
using Core;
using DTOs;
using DTOs.MessagingDTOs;
using Application.Interfaces.Services;

public class GetMessagesBatch
{
    public class Query : IRequest<Result<List<MessageDto>>>
    {
        public required string ChatId { get; set; }
        public required string UserId { get; set; }
        public PageDto Page { get; set; } = null!;
    }

    // TODO: Look at it once more because it is not absolutely complete I guess
    public class Handler(IMessagingService messagingService) : IRequestHandler<Query, Result<List<MessageDto>>>
    {
        public async Task<Result<List<MessageDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await messagingService.GetMessagesBatchAsync(request.ChatId, request.UserId, request.Page.Page,
                request.Page.PageSize, cancellationToken);
        }
    }
}