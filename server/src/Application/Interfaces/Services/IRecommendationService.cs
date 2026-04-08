using Application.Core;
using Application.DTOs.PublicationDTOs;

namespace Application.Interfaces.Services;

public interface IRecommendationService
{
    Task<Result<List<PublicationCardDto>>> GetRecommendations(string userId, int page, int pageSize,
        CancellationToken ct);
}