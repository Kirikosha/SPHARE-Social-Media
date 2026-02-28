using Domain.DTOs;

namespace Application.Repositories.UserActivityLogRepository;

public interface IUserActivityLogRepository
{
    Task<bool> LogUserResetRating(UserActionLogDto logDto);
}