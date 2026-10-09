using System.Security.Cryptography;
using System.Text;
using Library.Application.Common;
using Library.Application.Contracts.Auth;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Library.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly LibraryDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _settings;

    public AuthService(
        LibraryDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> settings)
    {

        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _settings = settings.Value;

    }

    public async Task<AuthLoginResult> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {

        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            
            throw new AuthenticationFailedException("Username and password are required.");

        // read-only projection: only the columns login needs, roles joined in the same query
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Username == dto.Username)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Password,
                Roles = u.UserRoleBridges.Select(b => b.UserRole!.Role).ToList(),
            })
            .FirstOrDefaultAsync(ct);


        if (user is not null && !string.IsNullOrEmpty(user.Password) && _passwordHasher.Verify(dto.Password, user.Password))

        {

            var roles = user.Roles.Where(r => !string.IsNullOrEmpty(r)).Cast<string>().ToList();
            return await IssueTokensAsync(user.Id, user.Username ?? dto.Username, roles, userId: user.Id, clientId: null, ct);

        }

        var client = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Username == dto.Username)
            .Select(c => new { c.Id, c.Username, c.Password })
            .FirstOrDefaultAsync(ct);


        if (client is not null && !string.IsNullOrEmpty(client.Password) && _passwordHasher.Verify(dto.Password, client.Password))
        {

            return await IssueTokensAsync(client.Id, client.Username ?? dto.Username, ["Client"], userId: null, clientId: client.Id, ct);

        }

        throw new AuthenticationFailedException("Invalid username or password.");
    }

    public async Task<AuthLoginResult> RefreshAsync(string? refreshToken, CancellationToken ct = default)
    {
        
        var existing = await FindActiveTokenAsync(refreshToken, ct)
            ?? throw new AuthenticationFailedException("Refresh token is missing, invalid, or expired.");

        existing.RevokedAt = DateTime.UtcNow;

        string username;
        List<string> roles;
        int? userId = existing.UserId;
        int? clientId = existing.ClientId;
        int subjectId;

            



        if (existing.UserId is int uid)
        {
            var user = await _context.Users
                .Include(u => u.UserRoleBridges).ThenInclude(b => b.UserRole)
                .FirstOrDefaultAsync(u => u.Id == uid, ct)
                ?? throw new AuthenticationFailedException("User account no longer exists.");

            username = user.Username ?? string.Empty;

            roles = user.UserRoleBridges.Select(b => b.UserRole?.Role).Where(r => !string.IsNullOrEmpty(r)).Cast<string>().ToList();

            subjectId = user.Id;
        }


        else if (existing.ClientId is int cid)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == cid, ct)
                ?? throw new AuthenticationFailedException("Client account no longer exists.");

            username = client.Username ?? string.Empty;
            roles = ["Client"];
            subjectId = client.Id;
        }


        else
        {
            throw new AuthenticationFailedException("Refresh token is not linked to an account.");
        }

        // no save here: IssueTokensAsync's SaveChanges persists the revoke and the new token together
        return await IssueTokensAsync(subjectId, username, roles, userId, clientId, ct);

    }



    //It marks the refresh token as dead in the database, so it can't be used to get new tokens
    public async Task LogoutAsync(string? refreshToken, CancellationToken ct = default)
    {

        if (string.IsNullOrWhiteSpace(refreshToken))
            return;


        var hash = Hash(refreshToken);
        var now = DateTime.UtcNow;

        // one UPDATE ... WHERE (no load, no tracking); a no-op if the token is unknown, already revoked, or expired
        await _context.RefreshTokens
            .Where(t => t.Token == hash && t.RevokedAt == null && t.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTime?)now), ct);
    }

    public async Task<TokenStatusDto> GetTokenStatusAsync(string? refreshToken, CancellationToken ct = default)
    {
        var existing = await FindTokenAsync(refreshToken, ct);

        if (existing is null)
            return new TokenStatusDto { IsValid = false, IsExpired = true };

        var expired = existing.ExpiresAt <= DateTime.UtcNow;

        var valid = existing.RevokedAt is null && !expired;

        return new TokenStatusDto { IsValid = valid, IsExpired = expired };
    }

    private async Task<AuthLoginResult> IssueTokensAsync(

        int principalId, string username, IReadOnlyList<string> roles, int? userId, int? clientId, CancellationToken ct)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(new JwtPrincipalInfo(principalId, username, roles));

        var rawRefreshToken = _jwtTokenService.GenerateRefreshToken();

        var refreshExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays);

        _context.RefreshTokens.Add(new RefreshToken
        {

            Token = Hash(rawRefreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpiresAt,
            UserId = userId,
            ClientId = clientId,

        });
        await _context.SaveChangesAsync(ct);



        var response = new AuthResultDto
        {
            Token = accessToken.Token,
            UserId = principalId,
            ExpiresAt = accessToken.ExpiresAt,
            Username = username,
            Roles = roles,
        };



        return new AuthLoginResult(response, rawRefreshToken, refreshExpiresAt);
    }

    //was a cookie value even provided
    private Task<RefreshToken?> FindTokenAsync(string? refreshToken, CancellationToken ct)
    {
        // No cookie means no session yet, so don't bother querying
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Task.FromResult<RefreshToken?>(null);

        // Only the hash is stored, never the token itself
        var hash = Hash(refreshToken);
        return _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == hash, ct);
    }

    private async Task<RefreshToken?> FindActiveTokenAsync(string? refreshToken, CancellationToken ct)
    {
        var token = await FindTokenAsync(refreshToken, ct);
        return token is { RevokedAt: null } && token.ExpiresAt > DateTime.UtcNow ? token : null;
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}






