namespace Application.Features.Publications.Commands;

using Core;
using Infrastructure;
using System.Threading;

public class SetPublicationSentState
{
    public class Query : IRequest<Result<Unit>>
    {
        public required string Id { get; set; }
        public required bool State { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Query request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications.FindAsync(request.Id, cancellationToken);
            if (publication == null)
                return Result<Unit>.Failure("Publication was not found", 404);

            publication.WasSent = request.State;
            context.Publications.Update(publication);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
