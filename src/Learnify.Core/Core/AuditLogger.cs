using Learnify.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Learnify.Core.Core;

public interface IAuditLogger
{
    Task LogAsync(string action, string entityType, string entityId, object? before = null, object? after = null, int? actorId = null);
}

public class AuditLogger : IAuditLogger
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditLogger> _logger;
    private readonly string _analyticsUrl;

    public AuditLogger(HttpClient httpClient, IConfiguration config, ILogger<AuditLogger> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        // Pointing to the internal service URL or gateway
        _analyticsUrl = config["Services:Analytics"] ?? "http://localhost:5008";
    }

    public async Task LogAsync(string action, string entityType, string entityId, object? before = null, object? after = null, int? actorId = null)
    {
        try
        {
            var entry = new AuditEntry
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                ActorId = actorId,
                Before = before != null ? System.Text.Json.JsonSerializer.Serialize(before) : null,
                After = after != null ? System.Text.Json.JsonSerializer.Serialize(after) : null,
                OccurredAt = DateTime.UtcNow
            };

            var response = await _httpClient.PostAsJsonAsync($"{_analyticsUrl}/api/admin/audit/manual-entry", entry);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to record audit log: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording audit log");
        }
    }
}
