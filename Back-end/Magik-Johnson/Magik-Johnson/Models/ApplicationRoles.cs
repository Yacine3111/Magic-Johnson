namespace Magik_Johnson.Models;

public static class ApplicationRoles
{
    public static readonly ApplicationRole Client = new("client");
    public static readonly ApplicationRole Employee = new("employee");
    public static readonly ApplicationRole Manager = new("manager");
}

public class ApplicationRole(string roleName)
{
    public string Value { get; } = roleName;
}
