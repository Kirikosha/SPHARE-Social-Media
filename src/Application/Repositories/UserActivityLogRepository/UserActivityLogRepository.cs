using Domain.DTOs;
using Infrastructure;

namespace Application.Repositories.UserActivityLogRepository;

public class UserActivityLogRepository(ApplicationDbContext context) : IUserActivityLogRepository
{
    public async Task<bool> LogUserAction(UserActionLogDto logDto)
    {
        UserActionLog log = new UserActionLog()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = logDto.UserId,
            AdditionalDescription = logDto.AdditionalDescription ?? string.Empty,
            Action = logDto.ActionType.ToString()
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