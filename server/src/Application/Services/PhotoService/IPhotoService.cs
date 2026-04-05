namespace Application.Services.PhotoService;

using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public interface IPhotoService
{
    Task<ImageUploadResult> AddProfilePhotoAsync(IFormFile image);
    Task<ImageUploadResult> AddPhotoAsync(IFormFile image);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}
