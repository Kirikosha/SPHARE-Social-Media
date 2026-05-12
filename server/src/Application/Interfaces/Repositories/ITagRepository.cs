using AutoMapper;
using Domain.Entities.RecomendationEntities;

namespace Application.Interfaces.Repositories;

public interface ITagRepository
{
    Task<int> AddTagAsync(Tag tag, CancellationToken ct);
    Task<List<int>> AddTagsAsync(List<Tag> tags, CancellationToken ct);
    Task AddTagsToPublicationAsync(string publicationId, List<Tag> tags, CancellationToken ct);
    Task SetTagForPublicationAsync(string publicationId, int tagId, CancellationToken ct);
    Task SetTagsForPublicationAsync(string publicationId, List<int> tagIds, CancellationToken ct);
    
    Task<Tag?> GetTagByIdAsync(int tagId, CancellationToken ct);
    Task<List<Tag>> GetTagsForPublicationAsync(string publicationId, CancellationToken ct);
    Task<List<Tag>> SearchTagsByNameAsync(string name, CancellationToken ct);

    Task DeleteTagAsync(int tagId, CancellationToken ct);
    Task DeleteTagConnectionAsync(int tagId, string publicationId, CancellationToken ct);

    Task<bool> IsTagExists(int tagId, CancellationToken ct);
    Task<bool> IsTagPublicationExists(int tagId, string publicationId, CancellationToken ct);
    Task<int> GetTagsAmountByPublicationIdAsync(string publicationId, CancellationToken ct);

    Task<(List<Tag> existing, List<Tag> newTags)> SplitExistingAndNewTagsAsync(
        List<Tag> tags, CancellationToken ct);

}