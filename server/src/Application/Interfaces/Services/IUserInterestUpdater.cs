using Application.Models;
using Domain.Entities.RecomendationEntities;

namespace Application.Interfaces.Services;

public interface IUserInterestUpdater
{
    Task<InterestUpdateResult> CalculateUpdatesAsync(
        IEnumerable<InterestSignal> signals,
        IEnumerable<UserInterestTag> existingInterests,
        CancellationToken ct);
}