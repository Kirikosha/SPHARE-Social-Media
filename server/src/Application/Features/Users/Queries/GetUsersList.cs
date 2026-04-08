
namespace Application.Features.Users.Queries;
using Core.Pagination;
using DTOs.UserDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetUsersList
{
    public class Query : IRequest<Result<PagedList<AdminUserDto>>>
    {
        public required PaginationParams PaginationParams { get; set; }
    }

    public class Handler(IUserService userService) 
        : IRequestHandler<Query, Result<PagedList<AdminUserDto>>>
    {
        public async Task<Result<PagedList<AdminUserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await userService.GetUserListAsync(request.PaginationParams, cancellationToken);
        }
    }
}
