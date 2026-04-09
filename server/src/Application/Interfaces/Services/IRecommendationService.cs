using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;

namespace Application.Interfaces.Services;

public interface IRecommendationService
{
    Task<Result<PagedList<PublicationCardDto>>> GetRecommendations(string userId, int page, int pageSize,
        CancellationToken ct);
}