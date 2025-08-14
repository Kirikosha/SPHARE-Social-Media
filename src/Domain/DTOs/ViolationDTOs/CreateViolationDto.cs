namespace Domain.DTOs.ViolationDTOs;
public class CreateViolationDto
{
    public int ItemToRemoveId { get; set; } // Depending on the endpoint, either comment or publication will be deleted
    public string RemovalReason { get; set; } = string.Empty;
    public int ViolationScoreIncrease { get; set; }
}
