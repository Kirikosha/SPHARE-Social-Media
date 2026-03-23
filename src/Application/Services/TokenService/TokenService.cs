using System.Security.Cryptography;

namespace Application.Services.TokenService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(User user)
    {
        var tokenKey = config["TokenKey"] ?? throw new ArgumentNullException(nameof(config), "TokenKey was not found");

        if (tokenKey.Length < 64) throw new ArgumentException("Your token key needs to be longer");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var claims = new List<Claim>
        {
            new (ClaimTypes.Role, user.Role.ToString()),
            new (ClaimTypes.Name, user.UniqueNameIdentifier),
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public RefreshToken CreateRefreshToken()
    {
        return new RefreshToken()
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(7)
        };
    }
}
