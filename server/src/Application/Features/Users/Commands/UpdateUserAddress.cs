using Application.Core;
using Application.DTOs.DetailedUserInfoDTOs;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Services;
using FluentValidation;

namespace Application.Features.Users.Commands;

public class UpdateUserAddress
{
    public class Command : IRequest<Result<AddressDto>>
    {
        public required string UserId { get; set; }
        public required UpdateUserAddressDto UpdateModel { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.UpdateModel.City)
                .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

            RuleFor(x => x.UpdateModel.Country)
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");
        }
    }
    
    public class Handler(IUserService userService) : IRequestHandler<Command, Result<AddressDto>>
    {
        public async Task<Result<AddressDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await userService.SetUserAddressAsync(request.UpdateModel, request.UserId, cancellationToken);
        }
    }
}