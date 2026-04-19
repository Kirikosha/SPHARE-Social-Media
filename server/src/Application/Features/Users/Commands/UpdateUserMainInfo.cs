using Application.Core;
using Application.DTOs.UserDTOs;
using Application.Errors;
using Application.Interfaces.Services;

namespace Application.Features.Users.Commands;

public class UpdateUserMainInfo
{
    public class Command : IRequest<OneOf<PublicUserDto, UniqueNamesOptions, Error>>
    {
        public required UpdateUserMainInfoDto MainInfo { get; set; }
        public required string UserId { get; set; }
    }
    
    public class Handler(IUserService userService) 
        : IRequestHandler<Command, OneOf<PublicUserDto, UniqueNamesOptions, Error>>
    {
        public async Task<OneOf<PublicUserDto, UniqueNamesOptions, Error>> Handle(Command request, CancellationToken 
                cancellationToken)
        {
            var updateResult = await userService.UpdateUserMainInformation(request.MainInfo, request.UserId, 
                cancellationToken);

            if (updateResult.TryPickT0(out _, out var remainder))
            {
                try
                {
                    var userResult = await userService.GetPublicUserByIdAsync(request.UserId, cancellationToken);
                    if (userResult.IsSuccess)
                        return userResult.Value!;
                    return new Error(userResult.Error!, userResult.Code);
                }
                catch
                {
                    return UserErrors.ErrorDuringUserReceiving();
                }
            }

            return remainder.Match<OneOf<PublicUserDto, UniqueNamesOptions, Error>>(
                options => options,
                error   => error
            );

        }
    }
}