using System.Net.Http.Json;
using Application.Interfaces.Services;
using Application.Models;

namespace Infrastructure.Services;

public class MlTaggingService(HttpClient httpClient) : IMlTaggingService
{
    public async Task<TagExtractionResult?> ExtractTagsAsync(string text, List<string> existingTags, CancellationToken
            ct)
    {
        var request = new TagRequest
        {
            Text = text,
            ExistingTags = existingTags
        };

        var response = await httpClient.PostAsJsonAsync("/extract-tags", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TagExtractionResult>(ct);
    }
}