using Application.DTOs;

namespace Application.Interfaces.Repositories;

public interface IUserActivityLogRepository
{
    Task<bool> LogUserAction(UserActionLogDto logDto);
}