using Domain;
using Domain.Entities.RecomendationEntities;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.UserInterestsUpdateService;

public class UserInterestUpdateJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserInterestUpdateJob> _logger;

    public UserInterestUpdateJob(IServiceScopeFactory scopeFactory, ILogger<UserInterestUpdateJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Interest update job failed");
            }

            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var checkpoint = await dbContext.JobCheckpoints
            .FirstOrDefaultAsync(j => j.JobName == "InterestUpdate", ct);

        var since = checkpoint?.LastProcessedAt ?? DateTime.UtcNow.AddHours(-4);
        var runStarted = DateTime.UtcNow;


        var logs = await dbContext.UserLogs
            .Where(l => l.ExecutedAt >= since && l.ExecutedAt < runStarted
                                              && l.TargetId != null &&
                                              (l.Action == nameof(UserLogAction.LikePublication) ||
                                               l.Action == nameof(UserLogAction.DislikePublication) ||
                                               l.Action == nameof(UserLogAction.CreateComment)))
            .Select(l => new { l.UserId, l.TargetId, l.Action })
            .ToListAsync(ct);

        if (!logs.Any())
        {
            _logger.LogInformation("Interest update job: no new logs since {Since}", since);
            UpdateCursor(dbContext, checkpoint, runStarted);
            await dbContext.SaveChangesAsync(ct);
            return;
        }

        var publicationIds = logs.Select(l => l.TargetId!).Distinct().ToList();

        var tagsByPublication = await dbContext.PublicationTags
            .Where(pt => publicationIds.Contains(pt.PublicationId))
            .GroupBy(pt => pt.PublicationId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.TagId).ToList(), ct);

        var signals = new List<(string UserId, int TagId, float Weight)>();

        foreach (var log in logs)
        {
            if (!tagsByPublication.TryGetValue(log.TargetId!, out var tags)) continue;

            float weight = log.Action switch
            {
                nameof(UserLogAction.LikePublication) => InterestSignalWeights.Like,
                nameof(UserLogAction.DislikePublication) => InterestSignalWeights.Unlike,
                nameof(UserLogAction.CreateComment) => InterestSignalWeights.Comment,
                _ => 0f
            };
            
            foreach (var tagId in tags) 
                signals.Add((log.UserId, tagId, weight));
        }

        var aggregated = signals
            .GroupBy(s => (s.UserId, s.TagId))
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.TagId,
                TotalEarned = Math.Clamp(g.Sum(s => s.Weight), -0.5f, 0.8f)
            })
            .ToList();
        
        var affectedUserIds = aggregated.Select(a => a.UserId).Distinct().ToList();

        var existingInterests = await dbContext.UserInterestTags
            .Where(i => affectedUserIds.Contains(i.UserId))
            .ToListAsync(ct);

        var interestLookup = existingInterests
            .GroupBy(i => i.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        var toAdd = new List<UserInterestTag>();

        foreach (var userId in affectedUserIds)
        {
            var userInterests = interestLookup.GetValueOrDefault(userId) ?? [];

            foreach (var interest in userInterests)
                interest.Weight *= 0.85f;

            foreach (var signal in aggregated.Where(a => a.UserId == userId))
            {
                var existing = userInterests.FirstOrDefault(i => i.TagId == signal.TagId);

                if (existing != null)
                    existing.Weight = Math.Clamp(existing.Weight + signal.TotalEarned, 0f, 1f);
                else if (signal.TotalEarned > 0)   // don't create a row for a pure dislike
                    toAdd.Add(new UserInterestTag
                    {
                        UserId = userId,
                        TagId = signal.TagId,
                        Weight = signal.TotalEarned,
                        LastUpdated = DateTime.UtcNow
                    });
            }
        }

        var toPrune = existingInterests.Where(i => i.Weight < 0.05f).ToList();
        dbContext.UserInterestTags.RemoveRange(toPrune);
        dbContext.UserInterestTags.AddRange(toAdd);
 
        UpdateCursor(dbContext, checkpoint, runStarted);

        await dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Interest update job completed. Processed {Count} logs", logs.Count);
    }
    
    private static void UpdateCursor(ApplicationDbContext context, JobCheckpoint? checkpoint, DateTime runStarted)
    {
        if (checkpoint == null)
            context.JobCheckpoints.Add(new JobCheckpoint { JobName = "InterestUpdate", LastProcessedAt = runStarted });
        else
            checkpoint.LastProcessedAt = runStarted;
    }
}