using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface ICloudinaryService
{
    Task<ImageUploadResult> AddProfilePhotoAsync(IFormFile image, string userId);
    Task<ImageUploadResult> AddProfilePhotoAsyncWithReplacement(IFormFile image, string userId, string publicId);
    Task<ImageUploadResult> AddPhotoAsync(IFormFile image);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}
