namespace Application.Features.Users.Queries;

using Application.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetUserEmailsByIds
{
    public class Query : IRequest<Result<List<string>>>
    {
        public required List<int> Ids { get; set; }
    }

    public class Handler(ApplicationDbContext context) 
        : IRequestHandler<Query, Result<List<string>>>
    {
        public async Task<Result<List<string>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Result<List<string>>.Success(await context.Users
            .Where(a => request.Ids.Contains(a.Id))
            .Select(a => a.Email)
            .ToListAsync(cancellationToken));
        }
    }
}
