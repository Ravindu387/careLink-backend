using Microsoft.AspNetCore.Authorization;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/funding-reports")]
public class FundingReportsController : ControllerBase
{
    private readonly IFundingReportsService _service;

    public FundingReportsController(IFundingReportsService service)
    {
        _service = service;
    }

    // GET /api/funding-reports/analytics
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics([FromQuery] FundingAnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _service.GetAnalyticsAsync(parameters, cancellationToken);
        return Ok(result);
    }
}
