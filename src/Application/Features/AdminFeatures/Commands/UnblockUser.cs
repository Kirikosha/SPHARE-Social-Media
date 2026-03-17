namespace Application.Features.AdminFeatures.Commands;

using Core;
using Domain.Entities;
using Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class UnblockUser
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

                user.Blocked = false;
                user.BlockedAt = null;
                user.ViolationScore = 0;

                context.Users.Update(user);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Something went wrong during the process. Error: {ex.Message}", 500);
            }
        }
    }
}
