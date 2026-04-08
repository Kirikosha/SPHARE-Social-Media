namespace Application.Features.Users.Commands;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class UpdateViolationScore
{
    public class Command : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required int ScoreIncreaseValue { get; set; }
    }
    public class Handler(IUserService userService) 
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await userService
                .UpdateViolationScore(request.UserId, request.ScoreIncreaseValue, cancellationToken);
        }
    }
}
