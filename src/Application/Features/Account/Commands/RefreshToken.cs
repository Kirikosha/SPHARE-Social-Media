using Application.Core;
using Application.Services.TokenService;
using Domain.DTOs.AccountDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Commands;

public class RefreshToken
{
    public class Command : IRequest<Result<AccountClaimsDto>>
    {
        public required string Token { get; set; }
    }
    
    public class Handler(ApplicationDbContext context, ITokenService tokenService)
        : IRequestHandler<Command, Result<AccountClaimsDto>>
    {
        public async Task<Result<AccountClaimsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(rt => rt.Token == request.Token), cancellationToken);

            if (user == null)
                return Result<AccountClaimsDto>.Failure("Invalid refresh token", 401);

            var existing = user.RefreshTokens.Single(rt => rt.Token == request.Token);

            if (existing.IsExpired)
                return Result<AccountClaimsDto>.Failure("Refresh token expired, please log in again", 401);

            if (existing.IsRevoked)
            {
                context.RemoveRange(user.RefreshTokens);
                await context.SaveChangesAsync(cancellationToken);

                return Result<AccountClaimsDto>.Failure(
                    "Security violation detected, please log in again", 401);
            }

            existing.IsRevoked = true;
            var newRefreshToken = tokenService.CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result<AccountClaimsDto>.Success(new AccountClaimsDto
            {
                UniqueNameIdentifier = user.UniqueNameIdentifier,
                Username = user.Username,
                UserId = user.Id,
                Role = user.Role.ToString(),
                Token = tokenService.CreateToken(user),
                RefreshToken = newRefreshToken.Token,
                Blocked = user.Blocked
            });
        }
    }
}