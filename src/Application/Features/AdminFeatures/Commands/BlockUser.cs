namespace Application.Features.AdminFeatures.Commands;

using Application.Core;
using Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public class BlockUser
{
    public class Command : IRequest<Result<bool>>
    {
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                User? user = await context.Users.FindAsync(request.UserId);
                if (user == null) throw new Exception("User was not found");

                user.Blocked = true;
                user.BlockedAt = DateTime.UtcNow;
                context.Users.Update(user);

                return Result<bool>.Success(true);
            }
            catch
            {
                // TODO: ADD LOGGER
                return Result<bool>.Failure("Blocking user was not successful", 400);
            }
        }
    }
}
