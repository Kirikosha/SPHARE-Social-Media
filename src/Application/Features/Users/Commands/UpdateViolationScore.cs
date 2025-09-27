namespace Application.Features.Users.Commands;

using Application.Core;
using Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public class UpdateViolationScore
{
    const int VIOLATION_LIMIT = 20;
    public class Command : IRequest<Result<bool>>
    {
        public required int UserId { get; set; }
        public required int ScoreIncreaseValue { get; set; }
    }
    public class Handler(ApplicationDbContext context) 
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FindAsync(request.UserId);
            if (user == null) return Result<bool>.Failure("User was not found", 404);

            user.ViolationScore += request.ScoreIncreaseValue;

            if (user.ViolationScore >= VIOLATION_LIMIT)
            {
                user.Blocked = true;
                user.BlockedAt = DateTime.UtcNow;
            }
            context.Users.Update(user);
            return Result<bool>.Success(true);
        }
    }
}
