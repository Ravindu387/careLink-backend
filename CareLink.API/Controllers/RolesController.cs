using CareLink.API.Authorization;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Roles;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Retrieve all configured platform roles with user counts and permission summaries.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        var result = await _roleService.GetRolesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve details and assigned permissions for a specific role.
    /// </summary>
    [HttpGet("{id}")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(string id)
    {
        var result = await _roleService.GetRoleByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a custom operational role with defined initial permissions.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Users.ManagePermissions)]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailsDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailsDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto request)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponse<RoleDetailsDto>.FailureResult("Valid role name is required."));
        }

        var result = await _roleService.CreateRoleAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Update role details or metadata.
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Users.ManagePermissions)]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleRequestDto request)
    {
        var result = await _roleService.UpdateRoleAsync(id, request);
        if (!result.Success)
        {
            if (result.Message == "Role not found.")
                return NotFound(result);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a custom role if it is not a system role and has no active members.
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Users.ManagePermissions)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        if (!result.Success)
        {
            if (result.Message == "Role not found.")
                return NotFound(result);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieve permissions associated with a specific role.
    /// </summary>
    [HttpGet("{id}/permissions")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolePermissions(string id)
    {
        var result = await _roleService.GetRolePermissionsAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Replace the permission set assigned to a specific role.
    /// </summary>
    [HttpPut("{id}/permissions")]
    [HasPermission(Permissions.Users.ManagePermissions)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRolePermissions(string id, [FromBody] UpdateRolePermissionsRequestDto request)
    {
        if (request.Permissions == null)
        {
            return BadRequest(ApiResponse<bool>.FailureResult("Permissions payload is required."));
        }

        var result = await _roleService.UpdateRolePermissionsAsync(id, request);
        if (!result.Success)
        {
            if (result.Message == "Role not found.")
                return NotFound(result);
            return BadRequest(result);
        }

        return Ok(result);
    }
}
