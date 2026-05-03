using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;
using Domain.Entities.Publications;

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
    Task<Publication?> GetRawPublicationByIdAsync(string publicationId, CancellationToken ct);
    Task<List<PublicationDto>> GetPlannedPublicationsAsync(string userId, CancellationToken ct);
    Task<int> UpdatePublicationViewsAsync(string publicationId, CancellationToken ct);
    Task<bool> IsPublicationExistsAsync(string id, CancellationToken ct);
    Task<bool> UpdatePublicationContentAsync(UpdatePublicationContentDto updateContent,
        CancellationToken ct);

    Task<bool> UpdateConditionalPublicationAsync(UpdateConditionalPublicationDto updateDto, CancellationToken ct);
    Task<bool> UpdatePlannedPublicationAsync(UpdatePlannedPublicationDto updateDto, CancellationToken ct);
    Task<bool> IsUserAuthorAsync(string userId, string publicationId, CancellationToken ct);
    Task<bool> SetPublicationStateToSentAsync(string publicationId, CancellationToken ct);
    Task<PublicationNavigationProperties?> GetPublicationNavigationPropertiesAsync(string publicationId, 
        CancellationToken ct);
    Task DeletePublicationAsync(string publicationId, CancellationToken ct);
    Task AddPublication(Publication publication, CancellationToken ct);
}

public sealed class PublicationNavigationProperties
{
    public required string PublicationId { get; set; }
    public required string AuthorId { get; set; }
    public List<string>? ImageIds { get; set; }
}