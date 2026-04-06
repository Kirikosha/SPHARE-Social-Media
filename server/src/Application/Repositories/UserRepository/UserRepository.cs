using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async  Task<User?> GetUserByEmailWithRefreshTokensAsync(string email, CancellationToken ct)
    {
        return await _context.Users.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task AddRefreshTokenAsync(string userId, RefreshToken token, CancellationToken ct)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Created)
            .ToListAsync(ct);

        if (tokens.Count == 5)
        {
            var oldest = tokens.First();
            _context.RefreshTokens.Remove(oldest);
        }

        token.UserId = userId;
        await _context.RefreshTokens.AddAsync(token, ct);
    }

}