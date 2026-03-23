using Application.Repositories.UserActivityLogRepository;
using Domain.DTOs;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories.SpamRepository;

internal enum OperationResult
{
    Allowed,
    Cleared,
    OverTheSpamLimit
}

public class SpamRepository(ApplicationDbContext context, IUserActivityLogRepository logRepository) : ISpamRepository
{
    private const double NormalPublicationSpamValueIncrease = 0.1;
    private const double NormalCommentSpamValueIncrease = 0.08;
    private const double NormalLikeSpamValueIncrease = 0.01;
    private const double NormalComplaintSpamValueIncrease = 0.05;

    private const double SpamLimit = 1.0;

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

    public async Task<bool> MakePublication(string userId, CancellationToken ct = default)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAPublication, userId, ct);

        return response;
    }

    public async Task<bool> MakeComment(string userId, CancellationToken ct = default)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAComment, userId, ct);

        return response;
    }

    public async Task<bool> MakeLike(string userId, CancellationToken ct = default)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.LikeAPublication, userId, ct);

        return response;
    }
    
    public async Task<bool> MakeComplaint(string userId, CancellationToken ct = default)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAComplaint, userId, ct);

        return response;
    }
}