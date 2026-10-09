using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Library.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Library.Infrastructure.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _credentials;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public GeneratedToken GenerateAccessToken(JwtPrincipalInfo principal)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {

            new(JwtRegisteredClaimNames.Sub, principal.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, principal.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),



        };



        claims.AddRange(principal.Roles.Select(role => new Claim("role", role)));

        var token = new JwtSecurityToken(

            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: _credentials);

        return new GeneratedToken(_tokenHandler.WriteToken(token), expiresAt);
    }


    public string GenerateRefreshToken()
    {

        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

    }
}
