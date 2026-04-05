using Domain.Enums;

namespace Application.Services.UserActionLogger;

public interface IUserActionLogger<T>
{
    Task LogAsync(string userId, UserLogAction actionType, object?
        additionalData = null, string? targetId = null, CancellationToken ct = default);
}