namespace Application.Features.Users.Commands;

using Core;
using Application.Features.Images.Commands;
using Services.PhotoService;
using AutoMapper;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class UpdatePublicUser
{
    public class Command : IRequest<Result<PublicUserDto>>
    {
        public required UpdatePublicUserDto UpdateUserModel { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper, IPhotoService photoService,
        IMediator mediator) : IRequestHandler<Command, Result<PublicUserDto>>
    {
        public async Task<Result<PublicUserDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users
                .Include(a => a.ProfileImage)
                .Include(a => a.ProfileDetails)
                .Include(a => a.Address)
                .FirstOrDefaultAsync(x => x.Id == request.UpdateUserModel.Id, cancellationToken);

            if (user == null)
                return Result<PublicUserDto>.Failure("User was not found", 404);

            if (!string.IsNullOrWhiteSpace(request.UpdateUserModel.Username))
                user.Username = request.UpdateUserModel.Username;

            if (!string.IsNullOrWhiteSpace(request.UpdateUserModel.UniqueNameIdentifier))
                user.UniqueNameIdentifier = request.UpdateUserModel.UniqueNameIdentifier;

            if (request.UpdateUserModel.Action == Domain.Enums.ImageAction.New)
            {
                var response = await photoService.AddPhotoAsync(request.UpdateUserModel.ProfileImage!);
                if (response.Error != null)
                    return Result<PublicUserDto>.Failure("Image was not uploaded", 500);

                Image image = new Image { PublicId = response.PublicId, ImageUrl = response.Url.AbsoluteUri };

                if (user.ProfileImage != null)
                {
                    var result = await mediator.Send(new DeleteProfileImage.Command { PublicId = user.ProfileImage
                        .PublicId! });
                    if (!result.IsSuccess)
                        return Result<PublicUserDto>.Failure(result.Error!, result.Code);
                }

                user.ProfileImage = image;
            }
            else if (request.UpdateUserModel.Action == Domain.Enums.ImageAction.Delete && user.ProfileImage != null)
            {
                var result = await mediator.Send(new DeleteProfileImage.Command { PublicId = user.ProfileImage
                    .PublicId! });
                if (!result.IsSuccess)
                    return Result<PublicUserDto>.Failure(result.Error!, 500);
                user.ProfileImage = null;
            }

            if (request.UpdateUserModel.UserProfileDetails != null)
            {
                if (user.ProfileDetails == null)
                {
                    user.ProfileDetails = mapper.Map<UserProfileDetails>(request.UpdateUserModel.UserProfileDetails);
                    user.ProfileDetails.UserId = user.Id;
                    context.ProfileDetails.Add(user.ProfileDetails);
                }
                else
                {
                    mapper.Map(request.UpdateUserModel.UserProfileDetails, user.ProfileDetails);
                }
            }
            else if (user.ProfileDetails != null && request.UpdateUserModel.UserProfileDetails == null)
            {
                context.ProfileDetails.Remove(user.ProfileDetails);
                user.ProfileDetails = null;
            }

            if (request.UpdateUserModel.Address != null)
            {
                if (user.Address == null)
                {
                    user.Address = mapper.Map<Address>(request.UpdateUserModel.Address);
                    user.Address.UserId = user.Id;
                    context.Addresses.Add(user.Address);
                }
                else
                {
                    mapper.Map(request.UpdateUserModel.Address, user.Address);
                }
            }
            else if (user.Address != null && request.UpdateUserModel.Address == null)
            {
                context.Addresses.Remove(user.Address);
                user.Address = null;
            }

            return Result<PublicUserDto>.Success(mapper.Map<PublicUserDto>(user));
        }
    }
}
