namespace Application.Features.Account.Commands;
using Core;
using DTOs.AccountDTOs;
using Application.Interfaces.Services;

public class RefreshToken
{
    public class Command : IRequest<Result<AccountClaimsDto>>
    {
        public required string Token { get; set; }
    }
    
    public class Handler(IAccountService accountService)
        : IRequestHandler<Command, Result<AccountClaimsDto>>
    {
        public async Task<Result<AccountClaimsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await accountService.RefreshTokenAsync(request.Token, cancellationToken);
        }
    }
}