namespace Application.Features.Publications.Queries;

using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsToRemind
{
    public class Query : IRequest<List<Publication>>
    {
        public int LastId { get; set; }
        public int BatchSize { get; set; }
    }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, List<Publication>>
    {
        public async Task<List<Publication>> Handle(Query request, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            return await context.Publications
            .Include(a => a.Author).ThenInclude(a => a.ProfileImage)
            .Include(a => a.Images)
            .Where(p => p.RemindAt != null && p.RemindAt <= now && !p.WasSent && p.Id > request.LastId)
            .OrderBy(p => p.Id)
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);
        }
    }
}
