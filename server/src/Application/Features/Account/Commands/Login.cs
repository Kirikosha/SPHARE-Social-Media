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

    public class Handler(IAccountService accountService) : IRequestHandler<Command, Result<AccountClaimsDto>>
    {
        public async Task<Result<AccountClaimsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await accountService.LoginAsync(request.LoginModel, cancellationToken);
        }
    }
}
