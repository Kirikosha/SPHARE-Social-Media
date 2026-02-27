using Application.Core;
using Application.Services.PhotoService;
using Application.Services.SubscriptionService;
using Application.Services.TokenService;
using CloudinaryDotNet.Actions;
using Domain.DTOs.AccountDTOs;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Account.Commands;
public class Register
{
    public class Command : IRequest<Result<AccountClaimsDto>>
    {
        public required RegisterDto RegisterModel { get; set; } 
    }

    public class Handler(ApplicationDbContext context, ITokenService tokenService, IPhotoService photoService,
        ISubscriptionService subscriptionService) : IRequestHandler<Command, Result<AccountClaimsDto>>
    {
        const int RANDOM_NAME_IDENTIFIER_KEY_VALUE_LENGTH = 7;
        public async Task<Result<AccountClaimsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            bool exists = await context.Users
                .AnyAsync(a => string.Equals(a.Email, request.RegisterModel.Email), cancellationToken);

            if (exists)
                return Result<AccountClaimsDto>.Failure("User with specified email already exists", 400);

            using var hmac = new HMACSHA512();

            string uniqueNameIdentifier = await BuildUniqueNameIdentifier(request.RegisterModel.Username);

            var user = new User
            {
                Username = request.RegisterModel.Username,
                Email = request.RegisterModel.Email,
                UniqueNameIdentifier = uniqueNameIdentifier,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8
                    .GetBytes(request.RegisterModel.Password)),
                PasswordSalt = hmac.Key,
                Role = Roles.User
            };

            if (request.RegisterModel.Image != null)
            {
                var imageUploadResult = await photoService.AddPhotoAsync(request.RegisterModel.Image);

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
                await subscriptionService.CreateUserNodeAsync(user.Id);
            }
            catch (Exception ex)
            {
                return Result<AccountClaimsDto>.Failure(ex.Message, 500);
            }

            SpamRating rating = new SpamRating()
            {
                UserId = user.Id,
                SpamValue = 0.0
            };

            await context.SpamRatings.AddAsync(rating, cancellationToken);

            AccountClaimsDto account = new AccountClaimsDto
            {
                UniqueNameIdentifier = user.UniqueNameIdentifier,
                Username = user.Username,
                UserId = user.Id,
                Role = user.Role.ToString(),
                Token = tokenService.CreateToken(user),
                Blocked = false
            };

            return Result<AccountClaimsDto>.Success(account);
        }

        private async Task<string> BuildUniqueNameIdentifier(string username)
        {
            StringBuilder sb = new StringBuilder(username);
            bool nameIdentifierExists = await context.Users.AnyAsync(a => string.Equals(a.UniqueNameIdentifier, username)); 
            while (nameIdentifierExists)
            {
                sb.Append('-');
                sb.Append(GenerateRandomString());
                nameIdentifierExists = await context.Users.AnyAsync(a => string.Equals(a.UniqueNameIdentifier, username)); 
            }

            return sb.ToString();
        }

        private static string GenerateRandomString()
        {
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            int randCharValue;
            int randCaseValue;
            char letter;
            for (int i = 0; i < RANDOM_NAME_IDENTIFIER_KEY_VALUE_LENGTH; i++)
            {
                randCharValue = rand.Next(0, 26);
                randCaseValue = rand.Next(0, 1);

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
}
