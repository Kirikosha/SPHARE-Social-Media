namespace Domain.Entities.Publications;

public class PlannedPublication : Publication
{
    public DateTime PublishAt { get; set; }
    public bool WasSent { get; set; } 
}