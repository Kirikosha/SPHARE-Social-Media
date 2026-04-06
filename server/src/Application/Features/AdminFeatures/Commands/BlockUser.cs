using Application.Interfaces.Logger;
using Domain.Enums;

namespace Application.Features.AdminFeatures.Commands;

using Core;
using Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public class BlockUser
{
    public class Command : IRequest<Result<bool>>
    {
        public required string UserToBlockId { get; set; }
        public required string BlockedById { get; set; }
    }

    public class Handler(ApplicationDbContext context, IUserActionLogger<BlockUser> logRepository) : 
        IRequestHandler<Command, 
        Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                User? user = await context.Users.FindAsync(request.UserToBlockId);
                if (user == null) throw new Exception("User was not found");

                user.Blocked = true;
                user.BlockedAt = DateTime.UtcNow;
                context.Users.Update(user);

                await logRepository.LogAsync(request.BlockedById, UserLogAction.BlockUser, new
                {
                    info = $"User " +
                           $"{request.UserToBlockId} was blocked by {request.BlockedById} at {user.BlockedAt}"
                }, request.UserToBlockId, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch
            {
                return Result<bool>.Failure("Blocking user was not successful", 400);
            }
        }
    }
}
