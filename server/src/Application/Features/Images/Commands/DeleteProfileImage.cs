namespace Application.Features.Images.Commands;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class DeleteProfileImage
{
    public class Command : IRequest<Result<bool>>
    {
        public required string PublicId { get; set; }
    }

    public class Handler(IPhotoService photoService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await photoService.DeleteProfileImageAsync(request.PublicId, cancellationToken);
        }
    }
}
