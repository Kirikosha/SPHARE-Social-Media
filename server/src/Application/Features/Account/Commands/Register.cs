using FluentValidation;

namespace Application.Features.Account.Commands;
using Core;
using DTOs.AccountDTOs;
using Application.Interfaces.Services;
public class Register
{
    public class Command : IRequest<Result<AccountClaimsDto>>
    {
        public required RegisterDto RegisterModel { get; set; } 
    }

    public class RegisterCommandValidator : AbstractValidator<Command>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.RegisterModel.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
                .MaximumLength(20).WithMessage("Username cannot exceed 20 characters.")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");

            RuleFor(x => x.RegisterModel.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.RegisterModel.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]+").WithMessage("Password must contain at least one number.")
                .Matches(@"[-_!@#$%^&*+=?*.]+").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.RegisterModel.Image)
                .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
                .WithMessage("Profile image size cannot exceed 5 MB.");
        }
    }

    public class Handler(IAccountService accountService) : IRequestHandler<Command, Result<AccountClaimsDto>>
    {
        public async Task<Result<AccountClaimsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await accountService.RegisterAsync(request.RegisterModel, cancellationToken);
        }
    }
}
