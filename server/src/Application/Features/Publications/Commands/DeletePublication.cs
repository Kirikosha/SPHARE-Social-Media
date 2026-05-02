namespace Application.Features.Publications.Commands;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class DeletePublication
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string Id { get; set; }
        public required string UserId { get; set; }
    }

    public class Handler(IPublicationService publicationService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await publicationService.DeletePublicationAsync(request.Id, request.UserId, cancellationToken);
        }
    }
}
