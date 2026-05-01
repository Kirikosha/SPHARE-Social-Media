using Domain.Entities.Publications;

namespace Domain.Entities.Complaints;
public class PublicationComplaint : Complaint
{
    public string PublicationId { get; set; } = string.Empty;
    public Publication Publication { get; set; } = null!;
}
