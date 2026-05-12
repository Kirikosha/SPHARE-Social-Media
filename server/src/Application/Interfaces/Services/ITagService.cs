using Application.Core;
using Application.DTOs.TagDTOs;
using Domain.Entities.RecomendationEntities;

namespace Application.Interfaces.Services;

public interface ITagService
{
    Task<Result<Unit>> CreateTagAsync(string userId, string publicationId, CreateTagDto tagDto, CancellationToken ct);
    Task<Result<Unit>> CreateTagsAsync(string userId, string publicationId, CreateTagsDto tagsDto, CancellationToken ct);
    Task<Result<Unit>> SetTagAsync(string userId, string publicationId, int tagId, CancellationToken ct);
    Task<Result<Unit>> SetTagsAsync(string userId, string publicationId, List<int> tagsId, CancellationToken ct);

    Task<Result<List<TagDto>>> SearchTags(string searchName, CancellationToken ct);
    Task<Result<List<TagDto>>> GetTagsForPublicationAsync(string publicationId, CancellationToken ct);

    Task<Result<Unit>> DeleteTagPublicationAsync(string userId, string publicationId, int tagId, CancellationToken ct);

    Task<Result<Unit>> SaveGeneratedTags(string userId, string publicationId, List<GeneratedTagDto> tags,
        CancellationToken ct);
}