namespace Application.Services.SubscriptionService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISubscriptionService
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
