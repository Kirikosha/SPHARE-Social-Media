
namespace Application.Features.Publications.Queries;
using DTOs.PublicationDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsToRemind
{
    public class Query : IRequest<Result<List<PublicationNotificationDto>>>
    {
        public DateTime PostedAt { get; set; }
        public int BatchSize { get; set; }
    }
    public class Handler(IPublicationService publicationService) : IRequestHandler<Query, Result<List<PublicationNotificationDto>>>
    {
        public async Task<Result<List<PublicationNotificationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await publicationService.GetPublicationsToRemindAsync(request.PostedAt, request.BatchSize,
                cancellationToken);
        }
    }
}
