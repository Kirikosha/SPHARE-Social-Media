namespace Application.Features.Users.Commands;
using DTOs.UserDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

// TODO: Separate into different updates
public class UpdatePublicUser
{
    public class Command : IRequest<Result<PublicUserDto>>
    {
        public required UpdatePublicUserDto UpdateUserModel { get; set; }
    }

    public class Handler(IUserService userService) : IRequestHandler<Command, Result<PublicUserDto>>
    {
        public async Task<Result<PublicUserDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await userService.UpdateUser(request.UpdateUserModel, cancellationToken);
        }
    }
}
