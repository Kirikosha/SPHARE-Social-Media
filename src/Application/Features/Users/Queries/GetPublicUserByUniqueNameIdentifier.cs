using Domain.DTOs.DetailedUserInfoDTOs;

namespace Application.Features.Users.Queries;

using Core;
using AutoMapper;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicUserByUniqueNameIdentifier
{
    public class Query : IRequest<Result<PublicUserDto>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    //TODO: think about some sort of repository pattern since we are duplicating code here and in GetPublicUserById
    public class Handler(ApplicationDbContext context, IMapper mapper) 
        : IRequestHandler<Query, Result<PublicUserDto>>
    {
        public async Task<Result<PublicUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userDto = await context.Users
                .Where(u => u.UniqueNameIdentifier == request.UniqueNameIdentifier)
                .Select(u => new PublicUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    UniqueNameIdentifier = u.UniqueNameIdentifier,
                    JoinedAt = u.DateOfCreation.ToString("yyyy-MM-dd"),
                    ProfileImage = u.ProfileImage != null ? u.ProfileImage.ImageUrl : null,
                    Blocked = u.Blocked,
                    UserProfileDetails = u.ProfileDetails != null
                        ? new UserProfileDetailsDto
                        {
                            Id = u.ProfileDetails.Id,
                            Pronouns = u.ProfileDetails.Pronouns,
                            MainProfileDescription = u.ProfileDetails.MainProfileDescription,
                            Interests = u.ProfileDetails.Interests,
                            DateOfBirth = u.ProfileDetails.DateOfBirth
                        }
                        : null,
                    Address = u.Address != null
                        ? new AddressDto
                        {
                            Id = u.Address.Id,
                            City = u.Address.City,
                            Country = u.Address.Country
                        }
                        : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (userDto == null)
                return Result<PublicUserDto>.Failure("User was not found", 404);

            return Result<PublicUserDto>.Success(userDto);
        }
    }
}
