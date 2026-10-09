namespace Library.Domain.Entities;

public class UserRoles
{
    public int Id { get; set; }
    public string? Role { get; set; }

    public ICollection<UserRoleBridge> UserRoleBridges { get; set; } = new List<UserRoleBridge>();
}
