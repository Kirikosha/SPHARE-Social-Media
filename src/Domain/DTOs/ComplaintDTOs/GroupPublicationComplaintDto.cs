namespace Domain.DTOs.ComplaintDTOs;
public class GroupPublicationComplaintDto
{
    public string PublicationId { get; set; } = string.Empty;
    public int TotalComplaints { get; set; }
    public List<ReasonDto> Reasons { get; set; } = [];
}
