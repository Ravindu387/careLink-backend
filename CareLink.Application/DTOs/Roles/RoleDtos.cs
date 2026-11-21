namespace CareLink.Application.DTOs.Roles;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public int UsersCount { get; set; }
    public int PermissionsCount { get; set; }
    public bool IsSystemRole { get; set; }
    public string Status => IsSystemRole ? "Protected" : "Active";
}

public class RoleDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public int UsersCount { get; set; }
    public bool IsSystemRole { get; set; }
    public IList<string> Permissions { get; set; } = new List<string>();
}

public class CreateRoleRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public IList<string> Permissions { get; set; } = new List<string>();
}

public class UpdateRoleRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Purpose { get; set; }
}

public class UpdateRolePermissionsRequestDto
{
    public IList<string> Permissions { get; set; } = new List<string>();
}

public class UpdateUserRolesRequestDto
{
    public IList<string> Roles { get; set; } = new List<string>();
}
