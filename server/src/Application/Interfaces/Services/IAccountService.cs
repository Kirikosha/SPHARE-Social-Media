using Application.Core;
using Application.DTOs.AccountDTOs;

namespace Application.Interfaces.Services;

public interface IAccountService
{
    Task<Result<AccountClaimsDto>> RegisterAsync(RegisterDto registerDto, CancellationToken ct);
    Task<Result<AccountClaimsDto>> RefreshTokenAsync(string token, CancellationToken ct);
    Task<Result<AccountClaimsDto>> LoginAsync(LoginDto loginDto, CancellationToken ct);
}