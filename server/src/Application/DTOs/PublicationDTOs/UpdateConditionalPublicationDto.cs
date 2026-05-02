using Domain.Enums;

namespace Application.DTOs.PublicationDTOs;

public class UpdateConditionalPublicationDto
{
    public required string PublicationId { get; set; }
    public required ConditionType ConditionType { get; set; }
    public required int ConditionTarget { get; set; }
    public required ComparisonOperator ComparisonOperator { get; set; }
}