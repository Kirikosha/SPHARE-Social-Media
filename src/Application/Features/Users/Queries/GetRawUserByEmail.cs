namespace Application.Features.Users.Queries;

using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetRawUserByEmail
{
    public class Query : IRequest<User>
    {
        public required string Email { get; set; }
    }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, User>
    {
        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FirstOrDefaultAsync(a => a.Email == request.Email);
            if (user == null) throw new Exception("User was not found");
            return user;
        }
    }
}
