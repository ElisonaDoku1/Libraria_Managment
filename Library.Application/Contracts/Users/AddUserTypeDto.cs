namespace Library.Application.Contracts.Users;

public class AddUserTypeDto
{
    public string? UserTypeName { get; set; }
}

public class UserTypeDto
{
    public int Id { get; set; }
    public string? UserTypeName { get; set; }
}
