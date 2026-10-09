using System.Diagnostics;

namespace Library.Domain.Entities;

public class UserType
{
    public int Id { get; set; }
    public string? UserTypeName { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();



}







