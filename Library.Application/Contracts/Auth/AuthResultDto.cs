namespace Library.Application.Contracts.Auth;

public class AuthResultDto
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Username { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

public class TokenStatusDto
{
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
}
