namespace Library.Domain.Entities;

public class UserRoleBridge
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int UserRoleId { get; set; }
    public UserRoles? UserRole { get; set; }
}
