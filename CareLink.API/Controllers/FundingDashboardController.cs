using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/funding-dashboard")]
public class FundingDashboardController : ControllerBase
{
    private readonly IFundingDashboardService _service;

    public FundingDashboardController(IFundingDashboardService service)
    {
        _service = service;
    }

    // GET /api/funding-dashboard
    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var data = await _service.GetDashboardDataAsync(cancellationToken);
        return Ok(data);
    }
}
