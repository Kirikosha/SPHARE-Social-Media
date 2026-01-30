namespace Domain.DTOs.ComplaintDTOs;
public class GroupPublicationComplaintDto
{
    public int PublicationId { get; set; }
    public int TotalComplaints { get; set; }
    public List<ReasonDto> Reasons { get; set; } = [];
}
