namespace Application.Interfaces.Services;

public interface INeo4JSubscriptionService
{
    Task FollowAsync(string followerId, string followedId);
    Task UnfollowAsync(string followerId, string followedId);
    Task<List<string>> GetFollowersAsync(string userId);
    Task<List<string>> GetFollowingAsync(string userId);
    Task CreateUserNodeAsync(string userId);
    Task<bool> IsFollowing(string userId, string followedId);
    Task<int> GetFollowerCountAsync(string userId);
    Task<int> GetFollowingCountAsync(string userId);
}
