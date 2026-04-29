using Learnify.Core.Domain;
using Learnify.Analytics.API.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnify.Analytics.API.Endpoints;

[ApiController]
[Route("api/admin/audit")]
[Produces("application/json")]
[Authorize(Roles = "Administrator")]
public class AuditController : ControllerBase
{
    private readonly IAuditStore _auditStore;

    public AuditController(IAuditStore auditStore)
    {
        _auditStore = auditStore;
    }

    [HttpGet("logs")]
    [ProducesResponseType(typeof(IEnumerable<AuditEntry>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int count = 100)
    {
        var logs = await _auditStore.GetLatestLogsAsync(count);
        return Ok(logs);
    }

    [HttpPost("manual-entry")]
    [ProducesResponseType(typeof(AuditEntry), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateManualEntry([FromBody] AuditEntry entry)
    {
        entry.OccurredAt = DateTime.UtcNow;
        await _auditStore.RecordAsync(entry);
        return CreatedAtAction(nameof(GetAuditLogs), entry);
    }
}
