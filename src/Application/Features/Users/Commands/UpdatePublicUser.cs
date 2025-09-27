namespace Application.Features.Users.Commands;

using Application.Core;
using Application.Features.Images.Commands;
using Application.Services.PhotoService;
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
            User? user = await context.Users.Include(a => a.ProfileImage)
                .FirstOrDefaultAsync(x => x.Id == request.UpdateUserModel.Id);

            if (user == null)
                return Result<PublicUserDto>.Failure("User was not found", 404);

            User mappedUser = mapper.Map<User>(request.UpdateUserModel);

            if (request.UpdateUserModel.Action == Domain.Enums.ImageAction.New)
            {
                var response = await photoService.AddPhotoAsync(request.UpdateUserModel.ProfileImage);
                if (response.Error != null)
                    return Result<PublicUserDto>.Failure("Image was not uploaded", 500);

                Image image = new Image { PublicId = response.PublicId, ImageUrl = response.Url.AbsoluteUri };

                if (user.ProfileImage != null)
                {
                    var result = await mediator.Send(new DeleteProfileImage.Command { PublicId = user.ProfileImage.PublicId });

                    if (!result.IsSuccess)
                        return Result<PublicUserDto>.Failure(result.Error!, result.Code);
                }

                mappedUser.ProfileImage = image;
            }
            else if (request.UpdateUserModel.Action == Domain.Enums.ImageAction.Delete &&
                user.ProfileImage != null)
            {
                var result = await mediator.Send(new DeleteProfileImage.Command { PublicId = user.ProfileImage.PublicId });
                if (!result.IsSuccess)
                    return Result<PublicUserDto>.Failure(result.Error, 500);
            }
            context.Users.Update(mappedUser);
            return Result<PublicUserDto>.Success(mapper.Map<PublicUserDto>(mappedUser));
        }
    }
}
