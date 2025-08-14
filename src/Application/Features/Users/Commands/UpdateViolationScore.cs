namespace Application.Features.Users.Commands;

using Domain.Entities;
using Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class UpdateViolationScore
{
    const int VIOLATION_LIMIT = 20;
    public class Command : IRequest<bool>
    {
        public required int UserId { get; set; }
        public required int ScoreIncreaseValue { get; set; }
    }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FindAsync(request.UserId);
            if (user == null) return false;

            user.ViolationScore += request.ScoreIncreaseValue;

            if (user.ViolationScore >= VIOLATION_LIMIT)
            {
                user.Blocked = true;
                user.BlockedAt = DateTime.UtcNow;
            }
            context.Users.Update(user);
            return true;
        }
    }
}
