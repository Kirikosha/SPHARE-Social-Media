using Domain.Enums;

namespace Domain.Entities.Publications;

public class ConditionalPublication : Publication
{
    public ConditionType ConditionType { get; set; }
    public int ConditionTarget { get; set; }
    public ComparisonOperator ComparisonOperator { get; set; }
}