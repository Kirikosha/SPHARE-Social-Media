using Application.Interfaces.Services;

namespace Application.Features.Images.Commands;

using Core;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

public class UploadPublicationImages
{
    public class Command : IRequest<Result<List<Image>>>
    {
        public required List<IFormFile> Images { get; set; }
    }

    public class Handler(IPhotoService photoService) : IRequestHandler<Command, Result<List<Image>>>
    {
        public async Task<Result<List<Image>>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await photoService.UploadPublicationImages(request.Images, cancellationToken);
        }
    }
}
