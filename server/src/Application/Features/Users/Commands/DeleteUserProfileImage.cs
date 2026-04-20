using Application.Core;
using Application.Interfaces.Services;

namespace Application.Features.Users.Commands;

public class DeleteUserProfileImage
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string UserId { get; set; }
    }
    
    public class Handler(IPhotoService photoService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await photoService.DeleteProfileImageAsync(request.UserId, cancellationToken);

            return result;
        }
    }
}