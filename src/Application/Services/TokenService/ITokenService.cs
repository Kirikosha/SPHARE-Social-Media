namespace Application.Services.TokenService;

using Domain.Entities;

public interface ITokenService
{
    string CreateToken(User user);
    RefreshToken CreateRefreshToken();
}
