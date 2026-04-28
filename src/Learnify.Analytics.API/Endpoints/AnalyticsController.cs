using Learnify.Analytics.API.Application;
using Learnify.Analytics.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnify.Analytics.API.Endpoints;

[ApiController]
[Route("api/admin/analytics")]
[Produces("application/json")]
[Authorize(Roles = "Administrator")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(PlatformAnalytics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardAnalytics()
    {
        var analytics = await _analyticsService.GetPlatformAnalyticsAsync();
        return Ok(analytics);
    }

    [HttpGet("user-growth")]
    [ProducesResponseType(typeof(IEnumerable<TimeSeriesData>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserGrowth([FromQuery] int months = 12)
    {
        var data = await _analyticsService.GetUserGrowthAsync(months);
        return Ok(data);
    }

    [HttpGet("enrollment-trends")]
    [ProducesResponseType(typeof(IEnumerable<TimeSeriesData>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnrollmentTrends([FromQuery] int months = 12)
    {
        var data = await _analyticsService.GetEnrollmentTrendsAsync(months);
        return Ok(data);
    }

    [HttpGet("revenue-trends")]
    [ProducesResponseType(typeof(IEnumerable<TimeSeriesData>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueTrends([FromQuery] int months = 12)
    {
        var data = await _analyticsService.GetRevenueTrendsAsync(months);
        return Ok(data);
    }
}
