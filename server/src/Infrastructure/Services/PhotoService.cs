using Application.Core;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PhotoService(ICloudinaryService cloudinaryService, ApplicationDbContext context) : IPhotoService
{
    public async Task<Result<bool>> DeleteProfileImageAsync(string publicId, CancellationToken ct)
    {
        var result = await cloudinaryService.DeletePhotoAsync(publicId);
        if (result.Error != null)
            return Result<bool>.Failure($"Image was not deleted due to an error. Error: {result.Error}", 500);

        var image = await context.Images.FirstOrDefaultAsync(a => a.PublicId == publicId, ct);
        if (image == null) return Result<bool>.Failure($"Image does not exist in the database", 500);
        context.Images.Remove(image);
        return Result<bool>.Success(true); 
    }

    public async Task<Result<List<Image>>> UploadPublicationImages(List<IFormFile> images, CancellationToken ct)
    {
        var uploadedImages = new List<Image>();

        foreach (var image in images)
        {
            try
            {
                var response = await cloudinaryService.AddPhotoAsync(image);
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