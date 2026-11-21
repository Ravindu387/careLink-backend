namespace CareLink.Application.DTOs.Permissions;

public class PermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class PermissionCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public int Count => Permissions.Count;
    public IList<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}
