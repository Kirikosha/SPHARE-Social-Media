using Application.Core;
using Application.Services.TokenService;
using Domain.DTOs.AccountDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Account.Commands;

public class Login
{
    public class Command : IRequest<Result<AccountClaimsDto>>
    {
        public required LoginDto LoginModel { get; set; }
    }

    public class Handler(ApplicationDbContext context, ITokenService tokenService) : IRequestHandler<Command, Result<AccountClaimsDto>>
    {
        public async Task<Result<AccountClaimsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(a => string.Equals(a.Email, request.LoginModel.Email), cancellationToken);

            if (user == null)
                return Result<AccountClaimsDto>.Failure("User with specified email does not exist", 404);

            using var hmac = new HMACSHA512(user.PasswordSalt);

            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.LoginModel.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return Result<AccountClaimsDto>.Failure("Invalid credentials", 401);
            }

            var refreshToken = tokenService.CreateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync(cancellationToken);
            
            AccountClaimsDto account = new AccountClaimsDto
            {
                UniqueNameIdentifier = user.UniqueNameIdentifier,
                Username = user.Username,
                UserId = user.Id,
                Token = tokenService.CreateToken(user),
                RefreshToken = refreshToken.Token,
                Role = user.Role.ToString(),
                Blocked = user.Blocked
            };

            return Result<AccountClaimsDto>.Success(account);
        }
    }
}
