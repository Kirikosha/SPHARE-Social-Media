using Domain.Enums;

namespace Application.Services.UserActionLogger;

public interface IUserActionLogger<T>
{
    Task LogAsync(int userId, UserLogAction actionType, object?
        additionalData = null, CancellationToken ct = default);
}