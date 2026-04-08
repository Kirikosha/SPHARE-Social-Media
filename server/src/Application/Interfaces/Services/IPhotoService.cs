using Application.Core;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IPhotoService
{
    Task<Result<bool>> DeleteProfileImageAsync(string publicId, CancellationToken ct);
    Task<Result<List<Image>>> UploadPublicationImages(List<IFormFile> images, CancellationToken ct);
}