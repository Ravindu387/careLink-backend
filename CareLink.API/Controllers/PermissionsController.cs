using CareLink.API.Authorization;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Permissions;
using CareLink.Application.DTOs.Roles;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly IRoleService _roleService;

    public PermissionsController(IPermissionService permissionService, IRoleService roleService)
    {
        _permissionService = permissionService;
        _roleService = roleService;
    }

    /// <summary>
    /// Retrieve all platform operational permissions grouped by domain category.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PermissionCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _permissionService.GetAllPermissionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve permissions granted to a specific role.
    /// </summary>
    [HttpGet("roles/{roleId}")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        var result = await _roleService.GetRolePermissionsAsync(roleId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update permission mapping for a specific role.
    /// </summary>
    [HttpPut("roles/{roleId}")]
    [HasPermission(Permissions.Users.ManagePermissions)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdateRolePermissionsRequestDto request)
    {
        var result = await _roleService.UpdateRolePermissionsAsync(roleId, request);
        if (!result.Success)
        {
            if (result.Message == "Role not found.")
                return NotFound(result);
            return BadRequest(result);
        }

        return Ok(result);
    }
}
