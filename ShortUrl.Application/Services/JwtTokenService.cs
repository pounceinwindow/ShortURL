using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using URLShortener.Options;

namespace URLShortener.Services;

public sealed class JwtTokenService
{
    public string CreateToken(string email, TimeSpan? lifetime = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, email) };

        var jwt = new JwtSecurityToken(
            AuthOptions.Issuer,
            AuthOptions.Audience,
            claims,
            expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(24)),
            signingCredentials: new SigningCredentials(
                AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}