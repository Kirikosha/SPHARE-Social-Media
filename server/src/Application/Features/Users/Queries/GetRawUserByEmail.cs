namespace Application.Features.Users.Queries;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetRawUserByEmail
{
    public class Query : IRequest<Result<User>>
    {
        public required string Email { get; set; }
    }
    public class Handler(IUserService userService) : IRequestHandler<Query, Result<User>>
    {
        public async Task<Result<User>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await userService.GetUserByEmailAsync(request.Email, cancellationToken);
        }
    }
}
