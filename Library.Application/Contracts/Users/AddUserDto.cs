namespace Library.Application.Contracts.Users;

public class AddUserDto
{
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int UserTypeId { get; set; }
    public List<int>? RoleIds { get; set; }
}
