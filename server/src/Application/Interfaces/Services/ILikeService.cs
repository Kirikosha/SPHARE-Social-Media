using Application.Core;
using Application.DTOs.LikeDTOs;

namespace Application.Interfaces.Services;

public interface ILikeService
{
    Task<Result<bool>> IsLikedByAsync(string userId, string publicationId, CancellationToken ct);
    Task<Result<LikeDto>> LikePublicationAsync(string publicationId, string userId, CancellationToken ct);
}