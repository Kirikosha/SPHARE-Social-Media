using Application.Core;

namespace Application.Interfaces.Services;

public interface IRefreshTokenService
{
    Task<Result<RefreshToken>> AddOrUpdateRefreshToken(string userId, CancellationToken ct);
    Result<Unit> RemoveUserRefreshTokensAsync(ICollection<RefreshToken> tokens, CancellationToken ct);
}