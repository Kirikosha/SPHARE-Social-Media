namespace Application.Services.SubscriptionService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISubscriptionService
{
    Task FollowAsync(int followerId, int followedId);
    Task UnfollowAsync(int followerId, int followedId);
    Task<List<int>> GetFollowersAsync(int userId);
    Task<List<int>> GetFollowingAsync(int userId);
    Task CreateUserNodeAsync(int userId);
    Task<bool> IsFollowing(int userId, int followedId);
}
