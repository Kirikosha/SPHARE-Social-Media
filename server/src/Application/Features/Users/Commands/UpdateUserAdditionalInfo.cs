using Application.Core;
using Application.DTOs.DetailedUserInfoDTOs;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Services;
using FluentValidation;

namespace Application.Features.Users.Commands;

public class UpdateUserAdditionalInfo
{
    public class Command : IRequest<Result<UserProfileDetailsDto>>
    {
        public required UpdateUserAdditionalInfoDto UpdateModel { get; set; }
        public required string UserId { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.UpdateModel.Pronouns)
                .MaximumLength(50).WithMessage("Pronouns must not exceed 50 characters.");

            RuleFor(x => x.UpdateModel.MainProfileDescription)
                .MaximumLength(500).WithMessage("Main profile description must not exceed 500 characters.");

            RuleFor(x => x.UpdateModel.Interests)
                .Must(interests => interests is not { Count: > 20 })
                .WithMessage("You cannot specify more than 20 interests.");

            RuleForEach(x => x.UpdateModel.Interests)
                .NotEmpty().WithMessage("Interest cannot be empty.")
                .MaximumLength(30).WithMessage("Each interest must not exceed 30 characters.");

            RuleFor(x => x.UpdateModel.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past.")
                .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Please provide a valid date of birth.")
                .When(x => x.UpdateModel.DateOfBirth.HasValue); 
        }
    }
    
    public class Handler(IUserService userService) : IRequestHandler<Command, Result<UserProfileDetailsDto>>
    {
        public async Task<Result<UserProfileDetailsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await userService.UpdateUserAdditionalInfoAsync(request.UpdateModel, request.UserId, cancellationToken);
        }
    }
}