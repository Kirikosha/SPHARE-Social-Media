using System.Text.Json.Serialization;

namespace Application.Models;

public class TagExtractionResult
{
    [JsonPropertyName("matched_tags")]
    public List<string> MatchedTags { get; set; } = [];

    [JsonPropertyName("new_tags")]
    public List<string> NewTags { get; set; } = [];
}