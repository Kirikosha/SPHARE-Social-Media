namespace Application.Features.Users.Queries;
using Application.Interfaces.Services;
using Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetUserEmailsByIds
{
    public class Query : IRequest<Result<List<string>>>
    {
        public required List<string> Ids { get; set; }
    }

    public class Handler(IUserService userService) 
        : IRequestHandler<Query, Result<List<string>>>
    {
        public async Task<Result<List<string>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await userService.GetUserEmailsByIdsAsync(request.Ids, cancellationToken);
        }
    }
}
