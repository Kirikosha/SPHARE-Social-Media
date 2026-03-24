using Domain.Entities.Complaints;
using Domain.Entities.RecomendationEntities;
using Domain.Enums;

namespace Domain.Entities;

public class Publication
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Content { get; set; }
    public DateTime PostedAt { get; set; } = DateTime.UtcNow; // Creation date
    public PublicationTypes PublicationType { get; set; } = PublicationTypes.ordinary;
    public DateTime? RemindAt { get; set; }
    public bool WasSent { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Condition module
    public ConditionType? ConditionType { get; set; }
    public int? ConditionTarget { get; set; }
    public ComparisonOperator? ComparisonOperator { get; set; }
    
    // View count module
    public int ViewCount { get; set; }
    
    //Navigation properties
    public string AuthorId { get; set; } = string.Empty;
    public User Author { get; set; } = null!;
    public List<Like> Likes { get; set; } = [];
    public List<Image>? Images { get; set; }
    public List<Comment>? Comments { get; set; }
    public List<PublicationComplaint> PublicationComplaints { get; set; } = [];
    public List<PublicationTag> PublicationTags { get; set; } = [];
    public List<PublicationView> Views { get; set; } = []; 
}
