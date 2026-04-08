using Application.Interfaces.Services;
using Application.Models;
using Application.Settings;
using Domain.Entities.RecomendationEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class UserInterestUpdateService : IUserInterestUpdater
{
    private readonly InterestUpdateSettings _settings;
    private readonly ILogger<UserInterestUpdateService> _logger;

    public UserInterestUpdateService(
        IOptions<InterestUpdateSettings> options,
        ILogger<UserInterestUpdateService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }
    
    public Task<InterestUpdateResult> CalculateUpdatesAsync(
        IEnumerable<InterestSignal> signals,
        IEnumerable<UserInterestTag> existingInterests,
        CancellationToken ct)
    {
        var snapshots = existingInterests
            .Select(i => new { 
                i.UserId, 
                i.TagId, 
                Weight = i.Weight * _settings.DecayFactor 
            })
            .ToList();

        var aggregated = signals
            .GroupBy(s => (s.UserId, s.TagId))
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.TagId,
                TotalWeight = Math.Clamp(
                    g.Sum(s => s.Weight), 
                    _settings.MinWeight, 
                    _settings.MaxWeight)
            })
            .ToList();

        var updates = new List<InterestUpdateCommand>();
        var adds = new List<InterestUpdateCommand>();
        var removes = new List<(string UserId, int TagId)>();

        foreach (var signal in aggregated)
        {
            var existing = snapshots
                .FirstOrDefault(s => s.UserId == signal.UserId && s.TagId == signal.TagId);

            if (existing != null)
            {
                var newWeight = Math.Clamp(existing.Weight + signal.TotalWeight, 0f, 1f);
                if (newWeight < _settings.PruneThreshold)
                    removes.Add((existing.UserId, existing.TagId));
                else
                    updates.Add(new InterestUpdateCommand(
                        signal.UserId, signal.TagId, newWeight, DateTime.UtcNow));
            }
            else if (signal.TotalWeight > 0)
            {
                adds.Add(new InterestUpdateCommand(
                    signal.UserId, signal.TagId, signal.TotalWeight, DateTime.UtcNow));
            }
        }
        
        var signalledKeys = aggregated.Select(a => (a.UserId, a.TagId)).ToHashSet();

        foreach (var snapshot in snapshots.Where(s => !signalledKeys.Contains((s.UserId, s.TagId))))
        {
            if (snapshot.Weight < _settings.PruneThreshold)
                removes.Add((snapshot.UserId, snapshot.TagId));
        }

        return Task.FromResult(new InterestUpdateResult(adds, updates, removes));
    }
}