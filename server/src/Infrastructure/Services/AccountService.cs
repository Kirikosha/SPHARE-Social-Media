using System.Security.Cryptography;
using System.Text;
using Application.Core;
using Application.DTOs.AccountDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services.TokenService;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services;

public class AccountService(IUserRepository userRepository, IPhotoService photoService, ISpamRepository spamRepository,
    ISubscriptionService subscriptionService, ITokenService tokenService, IRefreshTokenService refreshTokenService) 
    : IAccountService
{
    private const int MaxActiveRefreshTokens = 5;
    public async Task<Result<AccountClaimsDto>> RegisterAsync(RegisterDto registerDto, CancellationToken ct)
    {
        bool exists = await userRepository.IsUserExistsByEmailAsync(registerDto.Email, ct);

        if (exists)
            return Result<AccountClaimsDto>.Failure("User with specified email already exists", 400);

        using var hmac = new HMACSHA512();

        string uniqueNameIdentifier = await userRepository.BuildUniqueNameIdentifier(registerDto.Username, ct);

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



        bool result = await userRepository.CreateUserAsync(user, ct);
        if (!result)
            return Result<AccountClaimsDto>.Failure("User was not created or something went wrong", 500);


        var subscriptionInitialisationResult = await subscriptionService.InitialiseSubscription(user.Id);
        if (!subscriptionInitialisationResult.IsSuccess)
            return Result<AccountClaimsDto>.Failure(subscriptionInitialisationResult.Error!, 
            subscriptionInitialisationResult.Code);
        
        if (registerDto.Image != null)
        {
            var imageUploadResult = await photoService.UploadUserProfilePicture(registerDto.Image, user.Id, ct);
            if (!imageUploadResult.IsSuccess)
            {
                return Result<AccountClaimsDto>.Failure(imageUploadResult.Error!, 500);
            }
        }

        var spamInitialisationResult = await spamRepository.CreateSpamRating(user.Id, ct);
        if (!spamInitialisationResult.IsSuccess)
        {
            return Result<AccountClaimsDto>.Failure(spamInitialisationResult.Error!, 500);
        }

        var refreshToken = await refreshTokenService.AddOrUpdateRefreshToken(user.Id, ct);
        if (!refreshToken.IsSuccess)
            return Result<AccountClaimsDto>.Failure(refreshToken.Error!, 500);

        AccountClaimsDto account = new AccountClaimsDto
        {
            UniqueNameIdentifier = user.UniqueNameIdentifier,
            Username = user.Username,
            UserId = user.Id,
            Role = user.Role.ToString(),
            Token = tokenService.CreateToken(user),
            RefreshToken = refreshToken.Value!.Token,
            Blocked = false
        };

        return Result<AccountClaimsDto>.Success(account);
    }

    public async Task<Result<AccountClaimsDto>> RefreshTokenAsync(string token, CancellationToken ct)
    {
        var user = await userRepository.GetUserByRefreshTokenAsync(token, ct);

        if (user == null)
            return Result<AccountClaimsDto>.Failure("Invalid refresh token", 401);

        var existing = user.RefreshTokens.Single(rt => rt.Token == token);

        if (existing.IsExpired)
            return Result<AccountClaimsDto>.Failure("Refresh token expired, please log in again", 401);

        if (existing.IsRevoked)
        {
            var tokenRes = refreshTokenService.RemoveUserRefreshTokensAsync(user.RefreshTokens, ct);
            if (!tokenRes.IsSuccess)
                return Result<AccountClaimsDto>.Failure(
                    "Security violation detected, please log in again", 401);
        }

        existing.IsRevoked = true;
        var newRefreshToken = tokenService.CreateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);

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
        User? user = await userRepository.GetUserByEmailWithRefreshTokensAsync(loginDto.Email, ct);

        if (user == null)
            return Result<AccountClaimsDto>.Failure("User with specified email does not exist", 404);

        using var hmac = new HMACSHA512(user.PasswordSalt);

        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
                return Result<AccountClaimsDto>.Failure("Invalid credentials", 401);
        }

        var refreshTokenResult = await refreshTokenService.AddOrUpdateRefreshToken(user.Id, ct);
        if (!refreshTokenResult.IsSuccess)
            return Result<AccountClaimsDto>.Failure(refreshTokenResult.Error!, refreshTokenResult.Code);
            
        AccountClaimsDto account = new AccountClaimsDto
        {
            UniqueNameIdentifier = user.UniqueNameIdentifier,
            Username = user.Username,
            UserId = user.Id,
            Token = tokenService.CreateToken(user),
            RefreshToken = refreshTokenResult.Value!.Token,
            Role = user.Role.ToString(),
            Blocked = user.Blocked
        };

        return Result<AccountClaimsDto>.Success(account);
    }
}