namespace Application.Features.Images.Commands;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class DeleteProfileImage
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string UserId { get; set; }
    }

    public class Handler(IUserService userService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await userService.DeleteProfileImageAsync(request.UserId, cancellationToken);
        }
    }
}
