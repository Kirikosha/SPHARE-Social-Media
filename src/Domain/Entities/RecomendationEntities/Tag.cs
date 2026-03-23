namespace Domain.Entities.RecomendationEntities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    
    public List<PublicationTag> Publications { get; set; } = [];
    public List<UserInterestTag> UserInterests { get; set; } = [];
}