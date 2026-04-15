namespace Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> AddRefreshToken(string userId, CancellationToken ct);
    Task<bool> RemoveStaleTokens(string userId, CancellationToken ct);
    Task<List<RefreshToken>> GetUserActiveRefreshTokens(string userId, CancellationToken ct);
    Task<List<RefreshToken>> GetUserStaleRefreshTokens(string userId, CancellationToken ct);
    Task RemoveUserRefreshTokensAsync(ICollection<RefreshToken> refreshTokens, CancellationToken ct);
}