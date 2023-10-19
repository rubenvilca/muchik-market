using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Security.Application.Abstractions.Authentication;
using Security.Application.Abstractions.Clock;
using Security.Domain.Users;

namespace Security.Infrastructure.Authentication;
internal sealed class JwtProvider : IJwtProvider
{
    private readonly JwtSettings _jwtSettings;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtProvider(
        IOptions<JwtSettings> jwtSettings,
        IDateTimeProvider dateTimeProvider)
    {
        _jwtSettings = jwtSettings.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public string GenerateToken(User user)
    {
        var siginingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                SecurityAlgorithms.HmacSha256
            );


        var claims = new[]{
             new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
             new Claim(JwtRegisteredClaimNames.GivenName, user.Email),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var securityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            claims: claims,
            signingCredentials: siginingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}
