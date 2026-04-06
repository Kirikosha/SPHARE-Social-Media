namespace Application.DTOs.ViolationDTOs;
public class CreateViolationDto
{
    // Depending on the endpoint, either comment or publication will be deleted
    public string ItemToRemoveId { get; set; } 
    public string RemovalReason { get; set; } = string.Empty;
    public int ViolationScoreIncrease { get; set; }
}
