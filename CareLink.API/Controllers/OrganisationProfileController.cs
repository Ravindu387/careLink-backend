using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/organisation-profile")]
[Route("api/partner/organisation-profile")]
[Route("api/partner/profile")]
public class OrganisationProfileController : ControllerBase
{
    private readonly IOrganisationProfileService _service;
    private readonly ILogger<OrganisationProfileController> _logger;

    public OrganisationProfileController(
        IOrganisationProfileService service,
        ILogger<OrganisationProfileController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the current partner organisation's profile, verification status, and capabilities.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(OrganisationProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganisationProfileDto>> GetProfile(
        [FromQuery] string orgId = "ORG-027",
        CancellationToken cancellationToken = default)
    {
        var profile = await _service.GetProfileAsync(orgId, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Updates details of the partner organisation's profile.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(OrganisationProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganisationProfileDto>> UpdateProfile(
        [FromBody] UpdateOrganisationProfileDto dto,
        [FromQuery] string orgId = "ORG-027",
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateProfileAsync(orgId, dto, cancellationToken);
        _logger.LogInformation("Organisation profile '{Id}' updated.", updated.Id);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the operational capabilities (personnel, fleet, stock categories, coverage districts).
    /// </summary>
    [HttpPut("capabilities")]
    [ProducesResponseType(typeof(OrganisationProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganisationProfileDto>> UpdateCapabilities(
        [FromBody] UpdateCapabilitiesDto dto,
        [FromQuery] string orgId = "ORG-027",
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateCapabilitiesAsync(orgId, dto, cancellationToken);
        _logger.LogInformation("Capabilities for organisation '{Id}' updated.", updated.Id);
        return Ok(updated);
    }

    /// <summary>
    /// Resets the organisation profile and capabilities back to the baseline verified partner seed.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(OrganisationProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganisationProfileDto>> ResetProfile(CancellationToken cancellationToken)
    {
        var reset = await _service.ResetDefaultProfileAsync(cancellationToken);
        _logger.LogInformation("Organisation profile reset to default seed.");
        return Ok(reset);
    }
}
