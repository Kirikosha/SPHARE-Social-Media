using Microsoft.EntityFrameworkCore;

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
            bool exists = await context.Publications.AnyAsync(c => c.Id == request.Id, cancellationToken);
            if (!exists)
                return Result<Unit>.Failure("Publication was not found", 404);

            await context.Publications.Where(a => a.Id == request.Id).ExecuteUpdateAsync(c => c.SetProperty(x => x
                .WasSent, request.State), cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
