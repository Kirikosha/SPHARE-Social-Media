using Application.Core;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IPhotoService
{
    Task<Result<Unit>> DeleteProfileImageAsync(string userId, CancellationToken ct);
    Task<Result<Unit>> DeletePublicationImagesAsync(List<string> imageIds, CancellationToken ct);
    Task<Result<List<Image>>> UploadPublicationImages(List<IFormFile> images, CancellationToken ct);
    Task<Result<Unit>> UploadUserProfilePicture(IFormFile imageFile, string userId, CancellationToken ct);
    Task<bool> IsProfileImageExists(string userId, CancellationToken ct);
}