namespace Domain.Entities.Complaints;
public class PublicationComplaint : Complaint
{
    public int PublicationId { get; set; }
    public Publication Publication { get; set; } = null!;
}
