using Application.Core;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Services;

namespace Application.Features.Users.Queries;

public class GetUpdateData
{
    public class Query : IRequest<Result<UserUpdateDataDto>>
    {
        public required string UserId { get; set; }
    }
    
    public class Handler(IUserService userService) : IRequestHandler<Query, Result<UserUpdateDataDto>>
    {
        public async Task<Result<UserUpdateDataDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await userService.FetchUserUpdateData(request.UserId, cancellationToken);
        }
    }
}