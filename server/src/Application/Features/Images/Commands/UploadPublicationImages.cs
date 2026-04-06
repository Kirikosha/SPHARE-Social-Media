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
            var uploadedImages = new List<Image>();

            foreach (var image in request.Images)
            {
                try
                {
                    var response = await photoService.AddPhotoAsync(image);
                    if (response.Error != null) continue;

                    uploadedImages.Add(new Image
                    {
                        PublicId = response.PublicId,
                        ImageUrl = response.Url.AbsoluteUri
                    });
                }
                catch (Exception ex)
                {
                    return Result<List<Image>>.Failure($"During image upload error arised. Error: {ex.Message}", 500);
                }
            }

            return Result<List<Image>>.Success(uploadedImages);
        }
    }
}
