namespace Library.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? EmployeeId { get; set; }

    public int UserTypeId { get; set; }
    public UserType? UserType { get; set; }

    public ICollection<UserRoleBridge> UserRoleBridges { get; set; } = new List<UserRoleBridge>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
