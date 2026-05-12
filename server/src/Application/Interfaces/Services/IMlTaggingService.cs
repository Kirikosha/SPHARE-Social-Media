using Application.Models;

namespace Application.Interfaces.Services;

public interface IMlTaggingService
{
    Task<TagExtractionResult?> ExtractTagsAsync(string text, List<string> existingTags, CancellationToken ct);
}