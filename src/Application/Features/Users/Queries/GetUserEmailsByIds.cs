namespace Application.Features.Users.Queries;

using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetUserEmailsByIds
{
    public class Query : IRequest<List<string>>
    {
        public required List<int> Ids { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, List<string>>
    {
        public async Task<List<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await context.Users
            .Where(a => request.Ids.Contains(a.Id))
            .Select(a => a.Email)
            .ToListAsync(cancellationToken);
        }
    }
}
