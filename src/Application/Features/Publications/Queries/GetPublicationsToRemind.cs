using Domain.Enums;

namespace Application.Features.Publications.Queries;

using Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsToRemind
{
    public class Query : IRequest<Result<List<Publication>>>
    {
        public DateTime PostedAt { get; set; }
        public int BatchSize { get; set; }
    }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<List<Publication>>>
    {
        public async Task<Result<List<Publication>>> Handle(Query request, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            return Result<List<Publication>>.Success(await context.Publications
            .Include(a => a.Author)
            //.Include(a => a.Images)
            .Where(p => ((p.RemindAt != null && p.RemindAt <= now) 
                         || (p.ConditionType != null 
                         && p.ComparisonOperator == ComparisonOperator.GreaterThanOrEqual 
                         && p.Author.SubscriberNumber >= p.ConditionTarget)) 
                         && !p.WasSent && p.PostedAt > request.PostedAt)
            .OrderBy(p => p.Id)
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken));
        }
    }
}
