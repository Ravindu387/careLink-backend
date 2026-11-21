using CareLink.API.Authorization;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Roles;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retrieve paginated users with optional search, role, status, tab, and organization filters.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<UserDetailsDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] UserListQueryDto query)
    {
        var result = await _userService.GetUsersAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve user overview statistics (total, active, pending, disabled, privileged).
    /// </summary>
    [HttpGet("stats")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<UserStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _userService.GetUserStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve details of a specific user by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(string id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create or invite a new user account with role and optional organization.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<UserDetailsDto>.FailureResult("Validation failed.", errors));
        }

        var result = await _userService.CreateUserAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Update an existing user's profile information, assigned role, or organization.
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Users.Update)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<UserDetailsDto>.FailureResult("Validation failed.", errors));
        }

        var result = await _userService.UpdateUserAsync(id, request);
        if (!result.Success)
        {
            if (result.Message == "User not found.")
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update assigned roles for a specific user.
    /// </summary>
    [HttpPut("{id}/roles")]
    [HasPermission(Permissions.Users.AssignRoles)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] UpdateUserRolesRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<bool>.FailureResult("Invalid request payload."));
        }

        var result = await _userService.UpdateUserRolesAsync(id, request);
        if (!result.Success)
        {
            if (result.Message == "User not found.")
                return NotFound(result);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieve effective permissions for a specific user.
    /// </summary>
    [HttpGet("{id}/permissions")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissions(string id)
    {
        var result = await _userService.GetUserPermissionsAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update or toggle user active status (Active / Disabled).
    /// </summary>
    [HttpPatch("{id}/status")]
    [HasPermission(Permissions.Users.Update)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateUserStatusRequestDto request)
    {
        var result = await _userService.UpdateUserStatusAsync(id, request.IsActive);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a user account.
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Users.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result.Success)
        {
            if (result.Message == "User not found.")
                return NotFound(result);
            return BadRequest(result);
        }

        return Ok(result);
    }
}
