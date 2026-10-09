namespace Library.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public ICollection<ClientOrderBridge> ClientOrderBridges { get; set; } = new List<ClientOrderBridge>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
