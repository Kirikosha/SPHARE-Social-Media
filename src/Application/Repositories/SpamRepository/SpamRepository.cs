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

    private async Task<string> VerifyThePermissionToMakeAction(UserActionType actionType, int userId)
    {
        var rating = await context.SpamRatings.FirstOrDefaultAsync(a => a.UserId == userId);
        if (rating == null) throw new Exception("There is no such record of user and spamratings");
        if (rating.SpamValue >= SpamLimit)
        {
            return "Forbidden";
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

        return "Allowed";
    }

    private async Task ResetSpamRating(int userId)
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

        await logRepository.LogUserResetRating(
            new UserActionLogDto() { UserId = userId, ExecutedAt = DateTime.UtcNow });
    }

    private async Task ResetQuery(int userId)
    {
        await context.SpamRatings.Where(a => a.UserId == userId).ExecuteUpdateAsync(setters =>
            setters.SetProperty(s => s.SpamValue, 0.0));
    }

    public async Task<string> MakePublication(int userId)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAPublication, userId);

        return response;
    }

    public async Task<string> MakeComment(int userId)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAComment, userId);

        return response;
    }

    public async Task<string> MakeLike(int userId)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.LikeAPublication, userId);

        return response;
    }
    
    public async Task<string> MakeComplaint(int userId)
    {
        await ResetSpamRating(userId);

        var response = await VerifyThePermissionToMakeAction(UserActionType.MakeAComplaint, userId);

        return response;
    }
}