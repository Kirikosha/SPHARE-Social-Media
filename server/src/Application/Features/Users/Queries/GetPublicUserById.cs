namespace Application.Features.Users.Queries;
using Core;
using DTOs.UserDTOs;
using Application.Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicUserById
{
    public class Query : IRequest<Result<PublicUserDto>>
    {
        public required string Id { get; set; }
    }

    public class Handler(IUserService userService) 
        : IRequestHandler<Query, Result<PublicUserDto>>
    {
        public async Task<Result<PublicUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await userService.GetPublicUserByIdAsync(request.Id, cancellationToken);
        }
    }
}
