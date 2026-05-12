namespace Application.Models;

public class TagRequest
{
    public string Text { get; set; } = string.Empty;
    public List<string> ExistingTags { get; set; } = [];
    public int MaxTags { get; set; } = 5;
}