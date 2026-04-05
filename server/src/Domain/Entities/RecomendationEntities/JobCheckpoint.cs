using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.RecomendationEntities;

public class JobCheckpoint
{
    [Key]
    public string JobName { get; set; } = string.Empty;
    public DateTime LastProcessedAt { get; set; }
}