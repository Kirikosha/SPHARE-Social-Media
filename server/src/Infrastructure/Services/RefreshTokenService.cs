using Application.Core;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Infrastructure.Services;

public class RefreshTokenService(IRefreshTokenRepository refreshTokenRepository) : IRefreshTokenService
{
    private const int MaxActiveRefreshTokens = 5;
    public async Task<Result<RefreshToken>> AddOrUpdateRefreshToken(string userId, CancellationToken ct)
    {

        var activeTokens = await refreshTokenRepository.GetUserActiveRefreshTokens(userId, ct);

        if (activeTokens.Count >= MaxActiveRefreshTokens)
        {
            var toRevoke = activeTokens.Take(activeTokens.Count - MaxActiveRefreshTokens + 1);
            foreach (var rt in toRevoke)
                rt.IsRevoked = true;
        }

        var staleResult = await refreshTokenRepository.RemoveStaleTokens(userId, ct);
        if (!staleResult)
            return Result<RefreshToken>.Failure("Stale tokens removal was unsuccessful", 500);


        var createdRefreshToken = await refreshTokenRepository.AddRefreshToken(userId, ct);
        if (createdRefreshToken == null)
            return Result<RefreshToken>.Failure("Refresh token addition was unsuccessful", 500);

        return Result<RefreshToken>.Success(createdRefreshToken);
    }

    public Result<Unit> RemoveUserRefreshTokensAsync(ICollection<RefreshToken> tokens, CancellationToken ct)
    {
        try
        {
            refreshTokenRepository.RemoveUserRefreshTokensAsync(tokens, ct);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception)
        {
            return Result<Unit>.Failure("Tokens weren't deleted", 500);
        }

    }
}