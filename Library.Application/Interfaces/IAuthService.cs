using Library.Application.Contracts.Auth;

namespace Library.Application.Interfaces;

public record AuthLoginResult(AuthResultDto Response, string RefreshToken, DateTime RefreshTokenExpiresAt);

public interface IAuthService
{
    Task<AuthLoginResult> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<AuthLoginResult> RefreshAsync(string? refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string? refreshToken, CancellationToken ct = default);
    Task<TokenStatusDto> GetTokenStatusAsync(string? refreshToken, CancellationToken ct = default);
}








