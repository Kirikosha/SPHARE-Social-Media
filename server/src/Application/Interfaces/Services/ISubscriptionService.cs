using Application.Core;
using Application.DTOs.UserDTOs;

namespace Application.Interfaces.Services;

public interface ISubscriptionService
{
    Task<Result<bool>> IsFollowing(string userId, string uniqueNameIdentifier, CancellationToken ct);
    Task<Result<int>> GetSubscriptionsCount(string uniqueNameIdentifier, CancellationToken ct);
    Task<Result<List<PublicUserDto>>> GetSubscriptions(string uniqueNameIdentifier, CancellationToken ct);
    Task<Result<int>> GetFollowersCount(string uniqueNameIdentifier, CancellationToken ct);
    Task<Result<List<PublicUserDto>>> GetFollowers(string uniqueNameIdentifier, CancellationToken ct);
    Task<Result<bool>> Subscribe(string userId, string followUserUniqueNameIdentifier, CancellationToken ct);
    Task<Result<bool>> Unsubscribe(string userId, string followUserUniqueNameIdentifier, CancellationToken ct);
    Task<Result<Unit>> InitialiseSubscription(string userId);
}