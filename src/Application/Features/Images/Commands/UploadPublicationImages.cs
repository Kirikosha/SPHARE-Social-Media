namespace Application.Features.Images.Commands;

using Application.Core;
using Application.Services.PhotoService;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

public class UploadPublicationImages
{
    public class Command : IRequest<Result<List<Image>>>
    {
        public required List<IFormFile> Images { get; set; }
    }

    public class Handler(ApplicationDbContext context, IPhotoService photoService) : IRequestHandler<Command, Result<List<Image>>>
    {
        public async Task<Result<List<Image>>> Handle(Command request, CancellationToken cancellationToken)
        {
            List<Image> uploadedImages = new List<Image>();
            List<string> successfulUploads = new List<string>();

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

                    successfulUploads.Add(response.PublicId);
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
