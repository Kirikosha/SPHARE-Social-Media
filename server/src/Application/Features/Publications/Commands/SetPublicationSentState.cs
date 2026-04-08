
namespace Application.Features.Publications.Commands;
using Application.Interfaces.Services;
using Core;
using System.Threading;

public class SetPublicationSentState
{
    public class Query : IRequest<Result<Unit>>
    {
        public required string Id { get; set; }
        public required bool State { get; set; }
    }

    public class Handler(IPublicationService publicationService) : IRequestHandler<Query, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await publicationService.SetPublicationSentStateAsync(request.Id, request.State, cancellationToken);
        }
    }
}
