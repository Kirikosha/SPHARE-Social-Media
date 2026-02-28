using Domain.DTOs;
using Infrastructure;

namespace Application.Repositories.UserActivityLogRepository;

public class UserActivityLogRepository(ApplicationDbContext context) : IUserActivityLogRepository
{
    public async Task<bool> LogUserResetRating(UserActionLogDto logDto)
    {
        UserActionLog log = new UserActionLog()
        {
            Id = Guid.NewGuid(),
            UserId = logDto.UserId,
            AdditionalDescription = logDto.AdditionalDescription ?? string.Empty,
            Action = "RatingReset"
        };

        try
        {
            await context.UserLogs.AddAsync(log);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}