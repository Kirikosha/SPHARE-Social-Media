namespace Application.Models;

public class TagExtractionResult
{
    public List<string> MatchedTags { get; set; } = [];
    public List<string> NewTags { get; set; } = [];
}