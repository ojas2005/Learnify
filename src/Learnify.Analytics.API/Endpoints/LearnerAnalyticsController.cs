using Learnify.Analytics.API.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Learnify.Analytics.API.Endpoints;

[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
[Authorize]
public class LearnerAnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public LearnerAnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("student/{learnerId:int}/stats")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentStats(int learnerId)
    {
        var callerId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        var role = User.FindFirstValue(ClaimTypes.Role);

        // Allow if the caller is the learner themselves or an administrator
        if (callerId != learnerId && role != "Administrator")
        {
            return Forbid();
        }

        var stats = await _analyticsService.GetLearnerStatsAsync(learnerId);
        return Ok(stats);
    }
}
