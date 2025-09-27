namespace Application.Features.Users.Queries;

using Application.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetRawUserByEmail
{
    public class Query : IRequest<Result<User>>
    {
        public required string Email { get; set; }
    }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<User>>
    {
        public async Task<Result<User>> Handle(Query request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);
            if (user == null)
                return Result<User>.Failure("User was not found", 404);
            return Result<User>.Success(user);
        }
    }
}
