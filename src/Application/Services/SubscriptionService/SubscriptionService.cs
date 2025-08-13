namespace Application.Services.SubscriptionService;

using Infrastructure.Neo4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SubscriptionService(INeo4jDataAccess neo4j) : ISubscriptionService
{

    public async Task<List<int>> GetFollowersAsync(int userId)
    {
        var query = @"
            MATCH (f:User)-[:FOLLOWS]->(u:User {id: $userId})
            RETURN f.id as id";

        var result = await neo4j.ExecuteReadListAsync(query, "id", new Dictionary<string, object>
        {
            {
                "userId", userId
            }
        });

        return result.Select(int.Parse).ToList();
    }

    public async Task<List<int>> GetFollowingAsync(int userId)
    {
        var query = @"
            MATCH (u:User {id: $userId})-[:FOLLOWS]->(f:User)
            RETURN f.id as id";

        var result = await neo4j.ExecuteReadListAsync(query, "id", new Dictionary<string, object>
        {
            { "userId", userId }
        });

        return result.Select(int.Parse).ToList();
    }

    public async Task FollowAsync(int followerId, int followedId)
    {
        var query = @"
            MATCH (a:User {id: $followerId}), (b:User {id: $followedId})
            MERGE (a)-[:FOLLOWS]->(b)";

        await neo4j.ExecuteWriteAsync(query, new Dictionary<string, object>
        {
            {"followerId", followerId },
            {"followedId", followedId }
        });
    }

    public async Task UnfollowAsync(int followerId, int followedId)
    {
        var query = @"
            MATCH (a:User {id: $followerId})-[r:FOLLOWS]->(b:User {id: $followedId})
            DELETE r";

        await neo4j.ExecuteWriteAsync(query, new Dictionary<string, object>
        {
            {"followerId", followerId },
            {"followedId", followedId }
        });
    }

    public async Task CreateUserNodeAsync(int userId)
    {
        var query = @"
        MERGE (u:User {id: $id})";

        await neo4j.ExecuteWriteAsync(query, new Dictionary<string, object>
        {
            { "id", userId }
        });
    }

    public async Task<bool> IsFollowing(int userId, int followedId)
    {
        var query = @"
        MATCH (a:User {id: $userId})-[:FOLLOWS]->(b:User {id: $followedId})
        RETURN COUNT(*) AS count";

        var result = await neo4j.ExecuteReadListAsync(query, "count", new Dictionary<string, object>
    {
        { "userId", userId },
        { "followedId", followedId }
    });

        return result.Count > 0 && int.Parse(result[0]) > 0;
    }
}
