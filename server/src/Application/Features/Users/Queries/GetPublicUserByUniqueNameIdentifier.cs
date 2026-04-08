namespace Application.Features.Users.Queries;
using DTOs.UserDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicUserByUniqueNameIdentifier
{
    public class Query : IRequest<Result<PublicUserDto>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }
    public class Handler(IUserService userService) 
        : IRequestHandler<Query, Result<PublicUserDto>>
    {
        public async Task<Result<PublicUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await userService.GetPublicUserByUniqueNameAsync(request.UniqueNameIdentifier, cancellationToken);
        }
    }
}
