using Application.Core;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SubscriptionService(ApplicationDbContext context, INeo4JSubscriptionService neo4JSubscriptionService,
    IMapper mapper) 
    : ISubscriptionService
{
    public async Task<Result<bool>> IsFollowing(string userId, string uniqueNameIdentifier, CancellationToken ct)
    {
        bool currentUserExists = await context.Users.AnyAsync(a => a.Id == userId, ct);
        if (!currentUserExists) return Result<bool>.Failure("User does not exist", 404);

        string? otherUserId = await context.Users.Where(x => x.UniqueNameIdentifier == uniqueNameIdentifier)
            .Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (string.IsNullOrEmpty(otherUserId)) return Result<bool>
            .Failure("User, whom you wnat to check if you are following does not exist", 400);
        try
        {
            bool isFollowing = await neo4JSubscriptionService.IsFollowing(userId, otherUserId);
            return Result<bool>.Success(isFollowing);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message, 500);
        }
    }

    public async Task<Result<int>> GetSubscriptionsCount(string uniqueNameIdentifier, CancellationToken ct)
    {
        string? userId = await context.Users.Where(x => x.UniqueNameIdentifier == uniqueNameIdentifier)
            .Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (string.IsNullOrEmpty(userId)) return Result<int>.Failure("Such user was not found", 400);

        try
        {
            var result = await neo4JSubscriptionService.GetFollowingCountAsync(userId);
            return Result<int>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(ex.Message, 500);
        }
    }

    public async Task<Result<List<PublicUserDto>>> GetSubscriptions(string uniqueNameIdentifier, CancellationToken ct)
    {
        string? userId = await context.Users.Where(x => x.UniqueNameIdentifier == uniqueNameIdentifier)
            .Select(x => x.Id).FirstOrDefaultAsync(ct);
            
        if (string.IsNullOrEmpty(userId)) return Result<List<PublicUserDto>>.Failure("User, for whom we are looking " +
            "for all the " +
            "subscriptions, does not exist", 400);

        try
        {
            var subscribedUserIds = await neo4JSubscriptionService.GetFollowingAsync(userId);
            var users = await context.Users.Include(a => a.ProfileImage)
                .Where(a => subscribedUserIds.Contains(a.Id))
                .ProjectTo<PublicUserDto>(mapper.ConfigurationProvider)
                .ToListAsync(ct);
            return Result<List<PublicUserDto>>.Success(users);
        }
        catch (Exception ex)
        {
            return Result<List<PublicUserDto>>.Failure(ex.Message, 500);
        }
    }

    public async Task<Result<int>> GetFollowersCount(string uniqueNameIdentifier, CancellationToken ct)
    {
        string? userId = await context.Users.Where(x => x.UniqueNameIdentifier == uniqueNameIdentifier)
            .Select(x => x.Id).FirstOrDefaultAsync(ct);
            
        if (string.IsNullOrEmpty(userId)) return Result<int>.Failure("Such user was not found", 400);

        try
        {
            var result = await neo4JSubscriptionService.GetFollowerCountAsync(userId);
            return Result<int>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(ex.Message, 500);
        }
    }

    public async Task<Result<List<PublicUserDto>>> GetFollowers(string uniqueNameIdentifier, CancellationToken ct)
    {
        string? userId = await context.Users.Where(x => x.UniqueNameIdentifier == uniqueNameIdentifier)
            .Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (string.IsNullOrEmpty(userId)) return Result<List<PublicUserDto>>
            .Failure("User, for whom we are looking for all the subscriptions, does not exist", 400);
            
        try
        {
            var subscribedUserIds = await neo4JSubscriptionService.GetFollowersAsync(userId);
            var users = await context.Users.Include(a => a.ProfileImage)
                .Where(a => subscribedUserIds.Contains(a.Id))
                .ProjectTo<PublicUserDto>(mapper.ConfigurationProvider)
                .ToListAsync(ct);
            return Result<List<PublicUserDto>>.Success(users);
        }
        catch (Exception ex)
        {
            return Result<List<PublicUserDto>>.Failure(ex.Message, 500);
        }
    }

    public async Task<Result<bool>> Subscribe(string userId,
        string followUserUniqueNameIdentifier, CancellationToken ct)
    {
        try
        {
            bool followerExists = await context.Users.AnyAsync(u => u.Id == userId, ct);
            if (!followerExists)
                return Result<bool>.Failure("User does not exist", 404);
                
            var targetUserId = await context.Users
                .Where(u => u.UniqueNameIdentifier == followUserUniqueNameIdentifier)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(ct);
            if (targetUserId == null)
                return Result<bool>.Failure("User you want to follow does not exist", 400);
            
            await neo4JSubscriptionService.FollowAsync(userId, targetUserId);

            int rowsUpdated = await context.Users
                .Where(u => u.Id == targetUserId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(u => u.SubscriberNumber, u => u.SubscriberNumber + 1),
                    ct);

            if (rowsUpdated == 0)
            {
                await neo4JSubscriptionService.UnfollowAsync(userId, targetUserId);
                return Result<bool>.Failure("User not found while updating count", 404);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message, 500);
        }
    }

    public async Task<Result<bool>> Unsubscribe(string userId, string followUserUniqueNameIdentifier, CancellationToken
        ct)
    {
        try
        {
            bool followerExists = await context.Users.AnyAsync(u => u.Id == userId, ct);
            if (!followerExists)
                return Result<bool>.Failure("User does not exist", 404);
                
            var targetUserId = await context.Users
                .Where(u => u.UniqueNameIdentifier == followUserUniqueNameIdentifier)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(ct);
            if (targetUserId == null)
                return Result<bool>.Failure("User you want to unfollow does not exist", 400);
            
            await neo4JSubscriptionService.UnfollowAsync(userId, targetUserId);

            int rowsUpdated = await context.Users
                .Where(u => u.Id == targetUserId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(u => u.SubscriberNumber, u => u.SubscriberNumber + 1),
                    ct);

            if (rowsUpdated == 0)
            {
                await neo4JSubscriptionService.FollowAsync(userId, targetUserId);
                return Result<bool>.Failure("User not found while updating count", 404);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message, 500);
        }
    }

    public async Task<Result<Unit>> InitialiseSubscription(string userId)
    {
        try
        {
            await neo4JSubscriptionService.CreateUserNodeAsync(userId);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception)
        {
            return Result<Unit>.Failure("Subscription service intiialisation was unsuccessful", 500);
        }
    }
}