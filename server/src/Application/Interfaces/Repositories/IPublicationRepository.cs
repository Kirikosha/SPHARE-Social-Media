using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;

namespace Application.Interfaces.Repositories;

public interface IPublicationRepository
{
    Task<List<PublicationNotificationDto>> GetPublicationsToRemindAsync(DateTime postedAt, int batchSize,
        CancellationToken ct);

    // TODO: Optimisation idea, set up the month here for pagination sort of
    Task<List<PublicationCalendarDto>> GetPublicationsForCalendarAsync(string userId, CancellationToken ct);
    Task<PagedList<PublicationCardDto>> GetBatchOfPublicationCardsDto(string uniqueName, string userId,
        int page, int pageSize, CancellationToken ct);

    Task<PublicationDto?> GetPublicationByIdAsync(string publicationId, string userId, CancellationToken ct);
    Task<List<PublicationDto>> GetPlannedPublicationsAsync(string userId, CancellationToken ct);
    Task<int> UpdatePublicationViewsAsync(string publicationId, CancellationToken ct);
    Task<bool> IsPublicationExistsAsync(string id, CancellationToken ct);
}