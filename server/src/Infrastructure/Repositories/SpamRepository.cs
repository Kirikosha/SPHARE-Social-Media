using Application.Core;
using Application.DTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SpamRepository(ApplicationDbContext context, IUserActivityLogRepository logRepository) : ISpamRepository
{
    private const double NormalPublicationSpamValueIncrease = 0.1;
    private const double NormalCommentSpamValueIncrease = 0.08;
    private const double NormalLikeSpamValueIncrease = 0.01;
    private const double NormalComplaintSpamValueIncrease = 0.05;

    // normal user
    private const int PublicationTimeLimit = 5; // in minutes
    private const int PublicationNumberLimit = 50;
    
    // new user
    private const int NewUserPublicationNumberLimit = 1;
    private const int NewUserPublicationTimeLimit = 10; // in minutes
    
    private const double SpamLimit = 1.0;

    public async Task<bool> MakePublication(string userId, CancellationToken ct)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAPublication, userId, ct);

        return response;
    }

    public async Task<bool> MakeComment(string userId, CancellationToken ct)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAComment, userId, ct);

        return response;
    }

    public async Task<bool> MakeLike(string userId, CancellationToken ct)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.LikeAPublication, userId, ct);

        return response;
    }
    
    public async Task<bool> MakeComplaint(string userId, CancellationToken ct)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAComplaint, userId, ct);

        return response;
    }

    public async Task<Result<Unit>> CreateSpamRating(string userId, CancellationToken ct)
    {
        try
        {
            SpamRating rating = new SpamRating()
            {
                UserId = userId,
                SpamValue = 0.0
            };

            await context.SpamRatings.AddAsync(rating, ct);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception)
        {
            return Result<Unit>.Failure("User registration was unsuccessful", 500);
        }
    }

    public async Task<bool> IsPublicationSpamming(string userId, DateOnly creationDate, CancellationToken ct)
    {
        bool isUserNew = creationDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            
        // Set limits based on user status
        int timeLimitMinutes = isUserNew ? NewUserPublicationTimeLimit : PublicationTimeLimit;
        int? maxPublications = isUserNew ? NewUserPublicationNumberLimit : PublicationNumberLimit;

        // BUG FIX: Subtract minutes to look into the past, not the future
        var cutOffTime = DateTime.UtcNow.AddMinutes(-timeLimitMinutes);

        // Run a single, clean query
        int recentPublicationCount = await context.Publications
            .CountAsync(a => a.AuthorId == userId && a.PostedAt >= cutOffTime, ct);

        return recentPublicationCount >= maxPublications;
    }

    private async Task<bool> VerifyThePermissionToMakeAction(UserActionType actionType, string userId, 
    CancellationToken ct = default)
    {
        var rating = await context.SpamRatings.FirstOrDefaultAsync(a => a.UserId == userId, ct);
        if (rating == null) throw new Exception("There is no such record of user and spamratings");
        if (rating.SpamValue >= SpamLimit)
        {
            return false;
        }

        switch (actionType)
        {
            case UserActionType.MakeAPublication:
                rating.SpamValue += NormalPublicationSpamValueIncrease;
                break;
            case UserActionType.MakeAComment:
                rating.SpamValue += NormalCommentSpamValueIncrease;
                break;
            case UserActionType.LikeAPublication:
                rating.SpamValue += NormalLikeSpamValueIncrease;
                break;
            case UserActionType.MakeAComplaint:
                rating.SpamValue += NormalComplaintSpamValueIncrease;
                break;
            default:
                // TODO: Can be set differently, for example can be added here a reset option
                rating.SpamValue += 0;
                break;
        }

        return true;
    }

    private async Task ResetSpamRating(string userId)
    {
        var rating = await context.SpamRatings.FirstOrDefaultAsync(a => a.UserId == userId);
        if (rating == null) throw new Exception("There is no such record of user and spamratings");

        var cutoff = DateTime.UtcNow.AddHours(-24);
        // getting last one
        var userLog = await context.UserLogs
            .Where(a => a.UserId == userId
                        && a.Action == "RatingReset")
            .OrderByDescending(a => a.ExecutedAt)
            .FirstOrDefaultAsync();

        if (userLog == null)
        {
            if (rating.SpamValue > 0.0)
                await ResetQuery(userId);
        }
        else
        {
            if (userLog.ExecutedAt < cutoff)
            {
                await ResetQuery(userId);
            }
        }

        await logRepository.LogUserAction(
            new UserActionLogDto() { UserId = userId, ExecutedAt = DateTime.UtcNow });
    }

    private async Task ResetQuery(string userId)
    {
        await context.SpamRatings.Where(a => a.UserId == userId).ExecuteUpdateAsync(setters =>
            setters.SetProperty(s => s.SpamValue, 0.0));
    }
}