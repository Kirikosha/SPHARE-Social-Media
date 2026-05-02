using Application.Core;
using Application.Errors;
using Application.Interfaces.Services;
using CloudinaryDotNet.Actions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PhotoService(ICloudinaryService cloudinaryService, ApplicationDbContext context) : IPhotoService
{
    public async Task<Result<Unit>> DeleteProfileImageAsync(string userId, CancellationToken ct)
    {
        var profileImage = await context.Images.Where(x => x.UserId == userId).FirstOrDefaultAsync(ct);
        if (profileImage == null)
            return Result<Unit>.Failure(ImageErrors.ImageIsAlreadyDeleted());
        var result = await cloudinaryService.DeletePhotoAsync(profileImage.PublicId);
        if (result.Error != null)
            return Result<Unit>.Failure($"Image was not deleted due to an error. Error: {result.Error}", 500);

        context.Images.Remove(profileImage);
        return Result<Unit>.Success(Unit.Value); 
    }

    public async Task<Result<Unit>> DeletePublicationImagesAsync(List<string> imageIds, CancellationToken ct)
    {
        var deletionResult = await cloudinaryService.DeletePhotosAsync(imageIds);
        if (deletionResult.Error != null)
            return Result<Unit>.Failure($"Images weren't deleted due to an error. Error: {deletionResult.Error}", 500);
        
        await context.Images
            .Where(x => imageIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);
        
        return Result<Unit>.Success(Unit.Value);
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

    public async Task<Result<Unit>> UploadUserProfilePicture(IFormFile imageFile, string userId, CancellationToken ct)
    {
        var userProfilePublicId = await context.Images.Where(u => u.UserId == userId).Select(x => x.PublicId)
            .FirstOrDefaultAsync(ct);
        try
        {
            ImageUploadResult uploadResult;
            if (string.IsNullOrEmpty(userProfilePublicId))
            {
                uploadResult = await cloudinaryService.AddProfilePhotoAsync(imageFile, userId);
            }
            else
            {
                uploadResult = await cloudinaryService.AddProfilePhotoAsyncWithReplacement(imageFile, userId, userProfilePublicId);
            }
            
            if (uploadResult.Error != null)
                return Result<Unit>.Failure($"Image was not deleted due to an error. Error: {uploadResult.Error}", 500);

            if (string.IsNullOrEmpty(userProfilePublicId))
            {
                Image image = new Image()
                {
                    ImageUrl = uploadResult.Url.AbsoluteUri,
                    PublicId = uploadResult.PublicId,
                    UserId = userId
                };

                await context.Images.AddAsync(image, ct);
            }
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception)
        {
            return Result<Unit>.Failure("There was a critical error in image loading", 500);
        }
    }

    public async Task<bool> IsProfileImageExists(string userId, CancellationToken ct)
    {
        return await context.Images.Where(u => u.Id == userId).AnyAsync(ct);
    }
}