using Application.DTOs.UserDTOs;
using Application.Interfaces.Services;

namespace Application.Features.Users.Queries;

using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetUsersBySearchString
{
    public class Query : IRequest<Result<List<PublicUserBriefDto>>>
    {
        public required string SearchString { get; set; }
    }

    public class Handler(IUserService userService) 
        : IRequestHandler<Query, Result<List<PublicUserBriefDto>>>
    {
        public async Task<Result<List<PublicUserBriefDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.SearchString))
                return Result<List<PublicUserBriefDto>>.Success(new List<PublicUserBriefDto>());
            
            return await userService.GetUsersBySearchString(request.SearchString, cancellationToken);
        }
    }
}
