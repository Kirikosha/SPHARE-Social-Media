using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;

namespace Application.Interfaces.Services;

public interface IPublicationService
{
    Task<Result<List<PublicationNotificationDto>>> GetPublicationsToRemindAsync(DateTime postedAt, int batchSize,
        CancellationToken ct);

    Task<Result<List<PublicationCalendarDto>>> GetPublicationsCalendarAsync(string userId, CancellationToken ct);

    Task<Result<PagedList<PublicationCardDto>>> GetPublicationsByUniqueNameIdentifierAsync(string uniqueName,
        string userId, int page, int pageSize, CancellationToken ct);

    Task<Result<PublicationDto>> GetPublicationByIdAsync(string publicationId, string userId, CancellationToken ct);
    Task<Result<List<PublicationDto>>> GetPlannedPublicationsAsync(string userId, CancellationToken ct);
    Task<Result<int>> UpdatePublicationViewsAsync(string publicationId, CancellationToken ct);

    Task<Result<PublicationDto>> UpdatePublicationAsync(UpdatePublicationDto updateDto, string userId,
        CancellationToken ct);

    Task<Result<PublicationDto>> UpdatePublicationContentAsync(UpdatePublicationContentDto updateContentDto,
        string userId, CancellationToken ct);

    Task<Result<Unit>> SetPublicationSentStateAsync(string publicationId, bool state, CancellationToken ct);
    Task<Result<Unit>> DeletePublicationAsync(string publicationId, CancellationToken ct);
    Task<Result<bool>> CreatePublicationAsync(CreatePublicationDto createDto, string creatorId, CancellationToken ct);
}