namespace Application.Models;

public record InterestUpdateResult(
    IReadOnlyList<InterestUpdateCommand> ToAdd,
    IReadOnlyList<InterestUpdateCommand> ToUpdate,
    IReadOnlyList<(string UserId, int TagId)> ToRemove);

public record InterestUpdateCommand(string UserId, int TagId, float Weight, DateTime LastUpdated);