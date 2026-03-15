using Application.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Publications.Commands;

public class UpdatePublicationViews
{
    public class Command : IRequest<Result<int>>
    {
        public required int PublicationId { get; set; }
    }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            await context.Publications.Where(p => p.Id == request.PublicationId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.ViewCount, p => p.ViewCount + 1), cancellationToken);

            var viewCount = await context.Publications.Where(a => a.Id == request.PublicationId)
                .Select(x => x.ViewCount).FirstOrDefaultAsync(cancellationToken);

            return Result<int>.Success(viewCount);
        }
    }
}