using Application.Interfaces.Services;
using Application.Models;
using Domain;
using Domain.Entities.RecomendationEntities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HostedServices;

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
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updater = scope.ServiceProvider.GetRequiredService<IUserInterestUpdater>();

        var (checkpoint, since, runStarted) = await FetchCheckpointAsync(db, ct);

        var logs = await FetchActivityLogsAsync(db, since, runStarted, ct);
        if (logs.Count == 0)
        {
            _logger.LogInformation("Interest update job: no new logs since {Since}", since);
            AdvanceCheckpoint(db, checkpoint, runStarted);
            await db.SaveChangesAsync(ct);
            return;
        }

        var tagsByPublication  = await FetchTagsByPublicationAsync(db, logs, ct);
        var signals            = BuildSignals(logs, tagsByPublication);
        var existingInterests  = await FetchExistingInterestsAsync(db, signals, ct);

        var result = await updater.CalculateUpdatesAsync(signals, existingInterests, ct);

        ApplyResult(db, result);
        AdvanceCheckpoint(db, checkpoint, runStarted);
        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Interest update completed. Logs: {Logs}, signals: {Signals}, added: {Added}, updated: {Updated}, pruned: {Pruned}",
            logs.Count, signals.Count, result.ToAdd.Count, result.ToUpdate.Count, result.ToRemove.Count);
    }
    
    private static async Task<(JobCheckpoint? Checkpoint, DateTime Since, DateTime RunStarted)>
        FetchCheckpointAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var runStarted = DateTime.UtcNow;
 
        var checkpoint = await db.JobCheckpoints
            .FirstOrDefaultAsync(j => j.JobName == "InterestUpdate", ct);
 
        var since = checkpoint?.LastProcessedAt ?? runStarted.AddHours(-4);
 
        return (checkpoint, since, runStarted);
    }


    private static async Task<List<ActivityLogDto>> FetchActivityLogsAsync(
        ApplicationDbContext db,
        DateTime since,
        DateTime before,
        CancellationToken ct)
    {
        return await db.UserLogs
            .Where(l =>
                l.ExecutedAt >= since &&
                l.ExecutedAt < before &&
                l.TargetId != null &&
                (l.Action == nameof(UserLogAction.LikePublication) ||
                 l.Action == nameof(UserLogAction.DislikePublication) ||
                 l.Action == nameof(UserLogAction.CreateComment)))
            .Select(l => new ActivityLogDto
            {
                UserId   = l.UserId,
                TargetId = l.TargetId!,
                Action   = l.Action
            })
            .ToListAsync(ct);
    }

    private static async Task<Dictionary<string, List<int>>> FetchTagsByPublicationAsync(
        ApplicationDbContext db,
        List<ActivityLogDto> logs,
        CancellationToken ct)
    {
        var publicationIds = logs.Select(l => l.TargetId).Distinct().ToList();
 
        return await db.PublicationTags
            .Where(pt => publicationIds.Contains(pt.PublicationId))
            .GroupBy(pt => pt.PublicationId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(x => x.TagId).ToList(),
                ct);
    }
    
    private static async Task<List<UserInterestTag>> FetchExistingInterestsAsync(
        ApplicationDbContext db,
        List<InterestSignal> signals,
        CancellationToken ct)
    {
        var affectedUserIds = signals.Select(s => s.UserId).Distinct().ToList();
 
        return await db.UserInterestTags
            .Where(i => affectedUserIds.Contains(i.UserId))
            .ToListAsync(ct);
    }

    private static List<InterestSignal> BuildSignals(
        List<ActivityLogDto> logs,
        Dictionary<string, List<int>> tagsByPublication)
    {
        var raw = new List<(string UserId, int TagId, float Weight)>();
 
        foreach (var log in logs)
        {
            if (!tagsByPublication.TryGetValue(log.TargetId, out var tagIds))
                continue;
 
            float weight = log.Action switch
            {
                nameof(UserLogAction.LikePublication)    => InterestSignalWeights.Like,
                nameof(UserLogAction.DislikePublication) => InterestSignalWeights.Unlike,
                nameof(UserLogAction.CreateComment)      => InterestSignalWeights.Comment,
                _                                        => 0f
            };
 
            if (weight == 0f) continue;
 
            foreach (var tagId in tagIds)
                raw.Add((log.UserId, tagId, weight));
        }
 
        return raw
            .GroupBy(s => (s.UserId, s.TagId))
            .Select(g => new InterestSignal(
                UserId:  g.Key.UserId,
                TagId:   g.Key.TagId,
                Weight:  Math.Clamp(g.Sum(s => s.Weight), -0.5f, 0.8f)))
            .ToList();
    }
    
    private static void ApplyResult(ApplicationDbContext db, InterestUpdateResult result)
    {
        var trackedInterests = db.UserInterestTags.Local
            .ToDictionary(i => (i.UserId, i.TagId));

        foreach (var (userId, tagId) in result.ToRemove)
        {
            if (trackedInterests.TryGetValue((userId, tagId), out var entity))
                db.UserInterestTags.Remove(entity);
        }

        foreach (var cmd in result.ToUpdate)
        {
            if (trackedInterests.TryGetValue((cmd.UserId, cmd.TagId), out var entity))
            {
                entity.Weight      = cmd.Weight;
                entity.LastUpdated = cmd.LastUpdated;
            }
        }

        foreach (var cmd in result.ToAdd)
        {
            db.UserInterestTags.Add(new UserInterestTag
            {
                UserId      = cmd.UserId,
                TagId       = cmd.TagId,
                Weight      = cmd.Weight,
                LastUpdated = cmd.LastUpdated
            });
        }
    }
    
    private static void AdvanceCheckpoint(
        ApplicationDbContext db,
        JobCheckpoint? checkpoint,
        DateTime runStarted)
    {
        if (checkpoint is null)
            db.JobCheckpoints.Add(new JobCheckpoint
            {
                JobName         = "InterestUpdate",
                LastProcessedAt = runStarted
            });
        else
            checkpoint.LastProcessedAt = runStarted;
    }

    private class ActivityLogDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? TargetId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}