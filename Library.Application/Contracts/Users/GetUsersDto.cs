namespace Library.Application.Contracts.Users;

public class GetUsersDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public string? Username { get; set; }
    public string? EmployeeId { get; set; }
    public string? UserType { get; set; }
    public List<string>? Roles { get; set; }
}
