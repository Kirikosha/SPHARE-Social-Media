namespace Application.Repositories.UserRepository;

public interface IUserRepository
{
    Task<User?> GetUserByEmailWithRefreshTokensAsync(string email, CancellationToken ct);
    Task AddRefreshTokenAsync(string userId, RefreshToken token, CancellationToken ct);
}