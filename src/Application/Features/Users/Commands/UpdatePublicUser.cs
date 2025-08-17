namespace Application.Features.Users.Commands;

using Application.Features.Images.Commands;
using Application.Services.PhotoService;
using AutoMapper;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class UpdatePublicUser
{
    public class Command : IRequest<PublicUserDto>
    {
        public required UpdatePublicUserDto UpdateUserModel { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper, PhotoService photoService,
        IMediator mediator) : IRequestHandler<Command, PublicUserDto>
    {
        public async Task<PublicUserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.Include(a => a.ProfileImage)
                .FirstOrDefaultAsync(x => x.Id == request.UpdateUserModel.Id);

            if (user == null) throw new Exception("User does not exist");

            User mappedUser = mapper.Map<User>(request.UpdateUserModel);

            if (request.UpdateUserModel.Action == Domain.Enums.ImageAction.New)
            {
                var response = await photoService.AddPhotoAsync(request.UpdateUserModel.ProfileImage);
                if (response.Error != null) throw new Exception("Image was not stored");

                Image image = new Image { PublicId = response.PublicId, ImageUrl = response.Url.AbsoluteUri };

                if (user.ProfileImage != null)
                {
                    bool result = await mediator.Send(new DeleteProfileImage.Command { PublicId = user.ProfileImage.PublicId });
                    if (!result) throw new Exception("Image was not deleted");
                }

                mappedUser.ProfileImage = image;
            }
            else if (request.UpdateUserModel.Action == Domain.Enums.ImageAction.Delete &&
                user.ProfileImage != null)
            {
                bool result = await mediator.Send(new DeleteProfileImage.Command { PublicId = user.ProfileImage.PublicId });
                if (!result) throw new Exception("Image was not deleted");
            }
            context.Users.Update(mappedUser);
            return mapper.Map<PublicUserDto>(mappedUser);
        }
    }
}
