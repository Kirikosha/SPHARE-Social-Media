using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IPhotoService
{
    Task<ImageUploadResult> AddProfilePhotoAsync(IFormFile image);
    Task<ImageUploadResult> AddPhotoAsync(IFormFile image);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}
