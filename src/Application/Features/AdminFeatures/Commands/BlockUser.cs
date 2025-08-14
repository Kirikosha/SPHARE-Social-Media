namespace Application.Features.AdminFeatures.Commands;

using Domain.Entities;
using Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class BlockUser
{
    public class Command : IRequest<bool>
    {
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                User? user = await context.Users.FindAsync(request.UserId);
                if (user == null) throw new Exception("User was not found");

                user.Blocked = true;
                user.BlockedAt = DateTime.UtcNow;
                context.Users.Update(user);

                return true;
            }
            catch
            {
                // TODO: ADD LOGGER
                return false;
            }
        }
    }
}
