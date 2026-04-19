using FluentValidation;

namespace Application.Features.Account.Commands;
using Core;
using DTOs.AccountDTOs;
using Application.Interfaces.Services;
public class Login
{
    public class Command : IRequest<Result<AccountClaimsDto>>
    {
        public required LoginDto LoginModel { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.LoginModel.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.LoginModel.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]+").WithMessage("Password must contain at least one number")
                .Matches(@"[\!\?\*\.\@\#\$\%\^\&\+\=]+")
                .WithMessage("Password must contain at least one special character.");
        }
    }

    public class Handler(IAccountService accountService) : IRequestHandler<Command, Result<AccountClaimsDto>>
    {
        public async Task<Result<AccountClaimsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await accountService.LoginAsync(request.LoginModel, cancellationToken);
        }
    }
}
