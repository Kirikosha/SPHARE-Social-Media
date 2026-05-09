using System.Security.Cryptography;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> AddRefreshToken(string userId, CancellationToken ct)
    {
        try
        {
            var refreshToken = CreateRefreshToken(userId);
            await context.AddAsync(refreshToken, ct);
            return refreshToken;
        }
        catch (Exception)
        {
            return null!;
        }
    }

    public async Task<bool> RemoveStaleTokens(string userId, CancellationToken ct)
    {
        try
        {
            var staleTokens = await GetUserStaleRefreshTokens(userId, ct);
            context.RefreshTokens.RemoveRange(staleTokens);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<List<RefreshToken>> GetUserActiveRefreshTokens(string userId, CancellationToken ct)
    {
        var tokens = await context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked && r.Expires > DateTime.UtcNow)
            .ToListAsync(ct);
        return tokens;
    }

    public async Task<List<RefreshToken>> GetUserStaleRefreshTokens(string userId, CancellationToken ct)
    {
        return await context.RefreshTokens
            .Where(rt => rt.UserId == userId &&
                         (rt.Expires < DateTime.UtcNow ||
                          rt.IsRevoked))
            .ToListAsync(ct);
    }

    public Task RemoveUserRefreshTokensAsync(ICollection<RefreshToken> refreshTokens, CancellationToken ct)
    {
        context.RemoveRange(refreshTokens, ct);
        return Task.CompletedTask;
    }

    public Task RemoveExcessTokens(List<RefreshToken> tokens, CancellationToken ct)
    {
        context.RefreshTokens.RemoveRange(tokens);
        return Task.CompletedTask;
    }

    private RefreshToken CreateRefreshToken(string userId)
    {
        return new RefreshToken()
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = userId
        };
    }
}