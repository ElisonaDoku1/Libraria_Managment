namespace Library.Application.Interfaces;

public record JwtPrincipalInfo(int Id, string Username, IReadOnlyList<string> Roles);

public record GeneratedToken(string Token, DateTime ExpiresAt);

public interface IJwtTokenService
{
    GeneratedToken GenerateAccessToken(JwtPrincipalInfo principal);
    string GenerateRefreshToken();
}
