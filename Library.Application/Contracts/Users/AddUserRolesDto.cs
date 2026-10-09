namespace Library.Application.Contracts.Users;

public class AddUserRolesDto
{
    public string? Role { get; set; }
}

public class UserRolesDto
{
    public int Id { get; set; }
    public string? Role { get; set; }
}
