using Application.Core;
using Application.DTOs.UserDTOs;
using Application.Errors;
using Application.Interfaces.Services;
using FluentValidation;

namespace Application.Features.Users.Commands;

public class UpdateUserMainInfo
{
    public class Command : IRequest<OneOf<PublicUserDto, UniqueNamesOption, Error>>
    {
        public required UpdateUserMainInfoDto MainInfo { get; set; }
        public required string UserId { get; set; }
    }
    
    public class Handler(IUserService userService) 
        : IRequestHandler<Command, OneOf<PublicUserDto, UniqueNamesOption, Error>>
    {
        public async Task<OneOf<PublicUserDto, UniqueNamesOption, Error>> Handle(Command request, CancellationToken 
                cancellationToken)
        {
            var updateResult = await userService.UpdateUserMainInformationAsync(request.MainInfo, request.UserId, 
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

            return remainder.Match<OneOf<PublicUserDto, UniqueNamesOption, Error>>(
                options => options,
                error   => error
            );

        }
    }
}