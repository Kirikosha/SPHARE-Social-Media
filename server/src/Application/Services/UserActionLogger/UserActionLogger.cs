using Application.Repositories.UserActivityLogRepository;
using Domain.DTOs;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Services.UserActionLogger;

public class UserActionLogger<T>(IUserActivityLogRepository logRepository, ILogger<T> logger) : IUserActionLogger<T>
{
    public async Task LogAsync(string userId, UserLogAction actionType, object?
        additionalData = null, string? targetId = null,  CancellationToken ct = default)
    {
        if (additionalData == null)
        {
            additionalData = new { info = "" };
        }
        var dto = new UserActionLogDto()
        {
            UserId = userId,
            ActionType = actionType,
            AdditionalDescription = JsonConvert.SerializeObject(additionalData),
            ExecutedAt = DateTime.UtcNow,
            TargetId = targetId
        };
            
        var res = await logRepository.LogUserAction(dto);

        if (!res)
        {
            logger.LogError(
                "Failed to log user action {ActionType} for UserId {UserId}",
                actionType,
                userId
            );
        }
    }
}