using System.Linq.Expressions;
using System.Text;
using Application.Core.Pagination;
using Application.DTOs.DetailedUserInfoDTOs;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext context;
    private const int RandomNameIdentifierKeyValueLength = 7;
    public UserRepository(ApplicationDbContext context)
    {
        this.context = context;
    }
    public async  Task<User?> GetUserByEmailWithRefreshTokensAsync(string email, CancellationToken ct)
    {
        return await context.Users.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task AddRefreshTokenAsync(string userId, RefreshToken token, CancellationToken ct)
    {
        var tokens = await context.RefreshTokens
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Created)
            .ToListAsync(ct);

        if (tokens.Count == 5)
        {
            var oldest = tokens.First();
            context.RefreshTokens.Remove(oldest);
        }

        token.UserId = userId;
        await context.RefreshTokens.AddAsync(token, ct);
    }

    public Task<PublicUserDto?> GetPublicUserByIdAsync(string id, CancellationToken ct)
    {
        return context.Users
            .Where(u => u.Id == id)
            .Select(ToPublicUserDto)
            .FirstOrDefaultAsync(ct);
    }

    public Task<PublicUserDto?> GetPublicUserByUniqueNameAsync(string uniqueName, CancellationToken ct)
    {
        return context.Users
            .Where(u => u.UniqueNameIdentifier == uniqueName)
            .Select(ToPublicUserDto)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public async Task<User?> GetUserByIdAsync(string id, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string token, CancellationToken ct)
    {
        return await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token), ct);
    }

    public async Task<List<string>> GetUserEmailsByIds(List<string> ids, CancellationToken ct)
    {
        return await context.Users
            .Where(a => ids.Contains(a.Id))
            .Select(a => a.Email)
            .ToListAsync(ct);
    }

    public async Task<List<PublicUserBriefDto>> SearchUsersBySearchStringAsync(string searchString, CancellationToken ct)
    {
        var users = await context.Users
            .Where(u => EF.Functions.TrigramsSimilarity(u.UniqueNameIdentifier, searchString) > 0.3)
            .OrderByDescending(u => EF.Functions.TrigramsSimilarity(u.UniqueNameIdentifier, searchString))
            .Select(u => new PublicUserBriefDto
            {
                Id = u.Id,
                Username = u.Username,
                UniqueNameIdentifier = u.UniqueNameIdentifier,
                ImageUrl = u.ProfileImage != null ? u.ProfileImage.ImageUrl : null,
                Blocked = u.Blocked
            })
            .ToListAsync(ct);
        return users;
    }

    public async Task<PagedList<AdminUserDto>> GetUserList(PaginationParams paginationParams, CancellationToken ct)
    {
        var query = context.Users
            .OrderBy(u => u.Username)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Username = u.Username,
                UniqueNameIdentifier = u.UniqueNameIdentifier,
                Email = u.Email,
                ProfileImageUrl = u.ProfileImage != null ? u.ProfileImage.ImageUrl : null,
                ViolationScore = u.ViolationScore,
                AmountOfViolations = u.Violations.Count,
                Blocked = u.Blocked,
                BlockedAt = u.BlockedAt
            });

        var pagedList = await query.ToPagedListAsync(paginationParams.PageNumber,
            paginationParams.PageSize, ct);
        return pagedList;
    }

    public void UpdateUser(User user, CancellationToken ct)
    {
        context.Users.Update(user);
    }

    public async Task<User?> GetUserForUpdateByIdAsync(string id, CancellationToken ct)
    {
        User? user = await context.Users
            .Include(a => a.ProfileImage)
            .Include(a => a.ProfileDetails)
            .Include(a => a.Address)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return user;
    }

    public async Task<bool> IsUserExistsByEmailAsync(string email, CancellationToken ct)
    {
        return await context.Users.AnyAsync(a => a.Email == email, ct);

    }

    public async Task<bool> CreateUserAsync(User user, CancellationToken ct)
    {
        await context.Users.AddAsync(user, ct);
        var result = await context.SaveChangesAsync(ct) > 0;

        return result;
    }

    public async Task<string> BuildUniqueNameIdentifier(string username, CancellationToken ct)
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

    private static readonly Expression<Func<User, PublicUserDto>> ToPublicUserDto = u => new PublicUserDto
    {
        Id                   = u.Id,
        Username             = u.Username,
        UniqueNameIdentifier = u.UniqueNameIdentifier,
        JoinedAt             = u.DateOfCreation.ToString("yyyy-MM-dd"),
        ImageUrl         = u.ProfileImage != null ? u.ProfileImage.ImageUrl : null,
        Blocked              = u.Blocked,
        UserProfileDetails   = u.ProfileDetails != null
            ? new UserProfileDetailsDto
            {
                Id                     = u.ProfileDetails.Id,
                Pronouns               = u.ProfileDetails.Pronouns,
                MainProfileDescription = u.ProfileDetails.MainProfileDescription,
                Interests              = u.ProfileDetails.Interests,
                DateOfBirth            = u.ProfileDetails.DateOfBirth
            }
            : null,
        Address = u.Address != null
            ? new AddressDto
            {
                Id      = u.Address.Id,
                City    = u.Address.City,
                Country = u.Address.Country
            }
            : null
    };
}