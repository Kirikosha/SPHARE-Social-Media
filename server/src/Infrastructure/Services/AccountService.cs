using System.Security.Cryptography;
using System.Text;
using Application.Core;
using Application.DTOs.AccountDTOs;
using Application.Interfaces.Services;
using Application.Services.TokenService;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AccountService(ApplicationDbContext context, ICloudinaryService cloudinaryService,
    INeo4JSubscriptionService neo4JSubscriptionService, ITokenService tokenService) : IAccountService
{
    private const int MaxActiveRefreshTokens = 5;
    private const int RandomNameIdentifierKeyValueLength = 7;
    public async Task<Result<AccountClaimsDto>> RegisterAsync(RegisterDto registerDto, CancellationToken ct)
    {
            bool exists = await context.Users
            .AnyAsync(a => string.Equals(a.Email, registerDto.Email), ct);

            if (exists)
                return Result<AccountClaimsDto>.Failure("User with specified email already exists", 400);

            using var hmac = new HMACSHA512();

            string uniqueNameIdentifier = await BuildUniqueNameIdentifier(registerDto.Username, ct);

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                UniqueNameIdentifier = uniqueNameIdentifier,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8
                    .GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key,
                Role = Roles.User
            };

            if (registerDto.Image != null)
            {
                var imageUploadResult = await cloudinaryService.AddPhotoAsync(registerDto.Image);

                if (imageUploadResult.Error != null)
                {
                    return Result<AccountClaimsDto>.Failure(imageUploadResult.Error.Message, 500);
                }

                Image newImage = new Image
                {
                    ImageUrl = imageUploadResult.Url.AbsoluteUri,
                    PublicId = imageUploadResult.PublicId
                };

                user.ProfileImage = newImage;
            }
            
            bool result = await CreateUser(user);
            if (!result)
                return Result<AccountClaimsDto>.Failure("User was not created or something went wrong", 500);

            try
            {
                await neo4JSubscriptionService.CreateUserNodeAsync(user.Id);
            }
            catch (Exception ex)
            {
                return Result<AccountClaimsDto>.Failure(ex.Message + ex.InnerException, 500);
            }

            SpamRating rating = new SpamRating()
            {
                UserId = user.Id,
                SpamValue = 0.0
            };

            await context.SpamRatings.AddAsync(rating, ct);

            var activeTokens = user.RefreshTokens
                .Where(rt => rt.IsActive)
                .OrderBy(rt => rt.Created)
                .ToList();

            if (activeTokens.Count >= MaxActiveRefreshTokens)
            {
                var toRevoke = activeTokens.Take(activeTokens.Count - MaxActiveRefreshTokens + 1);
                foreach (var rt in toRevoke)
                    rt.IsRevoked = true;
            }
        
            var stale = user.RefreshTokens
                .Where(rt => rt.IsExpired)
                .ToList();

            context.RemoveRange(stale);
            
            var refreshToken = tokenService.CreateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync(ct);

            AccountClaimsDto account = new AccountClaimsDto
            {
                UniqueNameIdentifier = user.UniqueNameIdentifier,
                Username = user.Username,
                UserId = user.Id,
                Role = user.Role.ToString(),
                Token = tokenService.CreateToken(user),
                RefreshToken = refreshToken.Token,
                Blocked = false
            };

            return Result<AccountClaimsDto>.Success(account);
    }

    public async Task<Result<AccountClaimsDto>> RefreshTokenAsync(string token, CancellationToken ct)
    {
        var user = await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u =>
                u.RefreshTokens.Any(rt => rt.Token == token), ct);

        if (user == null)
            return Result<AccountClaimsDto>.Failure("Invalid refresh token", 401);

        var existing = user.RefreshTokens.Single(rt => rt.Token == token);

        if (existing.IsExpired)
            return Result<AccountClaimsDto>.Failure("Refresh token expired, please log in again", 401);

        if (existing.IsRevoked)
        {
            context.RemoveRange(user.RefreshTokens);
            await context.SaveChangesAsync(ct);

            return Result<AccountClaimsDto>.Failure(
                "Security violation detected, please log in again", 401);
        }

        existing.IsRevoked = true;
        var newRefreshToken = tokenService.CreateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync(ct);

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

    public async Task<Result<AccountClaimsDto>> LoginAsync(LoginDto loginDto, CancellationToken ct)
    {
        User? user = await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(a => string.Equals(a.Email, loginDto.Email), ct);

        if (user == null)
            return Result<AccountClaimsDto>.Failure("User with specified email does not exist", 404);

        using var hmac = new HMACSHA512(user.PasswordSalt);

        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
                return Result<AccountClaimsDto>.Failure("Invalid credentials", 401);
        }

        var activeTokens = user.RefreshTokens
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.Created)
            .ToList();

        if (activeTokens.Count >= MaxActiveRefreshTokens)
        {
            var toRevoke = activeTokens.Take(activeTokens.Count - MaxActiveRefreshTokens + 1);
            foreach (var rt in toRevoke)
                rt.IsRevoked = true;
        }
        
        var stale = user.RefreshTokens
            .Where(rt => rt.IsExpired)
            .ToList();

        context.RemoveRange(stale);
        
        var refreshToken = tokenService.CreateRefreshToken();
        user.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(ct);
            
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

    private async Task<string> BuildUniqueNameIdentifier(string username, CancellationToken ct)
    {
        StringBuilder sb = new StringBuilder(username);
        bool nameIdentifierExists = await context.Users
            .AnyAsync(a => string.Equals(a.UniqueNameIdentifier, sb.ToString()), ct);
        
        while (nameIdentifierExists)
        {
            sb.Append('-');
            sb.Append(GenerateRandomString());
            nameIdentifierExists = await context.Users
                .AnyAsync(a => string.Equals(a.UniqueNameIdentifier, sb.ToString()), ct);
        }

        return sb.ToString();
    }

    private static string GenerateRandomString()
    {
        StringBuilder sb = new StringBuilder();
        int randCharValue;
        int randCaseValue;
        char letter;
    
        for (int i = 0; i < RandomNameIdentifierKeyValueLength; i++)
        {
            randCharValue = Random.Shared.Next(0, 26);
            randCaseValue = Random.Shared.Next(0, 2);

            letter = Convert.ToChar(randCharValue + 65);
            letter = randCaseValue == 0 ? char.ToLower(letter) : letter;

            sb.Append(letter);
        }

        return sb.ToString();
    }

    private async Task<bool> CreateUser(User user)
    {
        if (user.ProfileImage != null)
        {
            await context.Images.AddAsync(user.ProfileImage);
        }

        await context.Users.AddAsync(user);
        var result = await context.SaveChangesAsync() > 0;

        return result;
    }
}