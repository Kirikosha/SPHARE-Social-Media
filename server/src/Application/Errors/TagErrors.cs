using Application.Core;

namespace Application.Errors;

public static class TagErrors
{
    public static Error TagAmountLimitViolation() => new Error("You cannot add more than 20 tags", 400);
}