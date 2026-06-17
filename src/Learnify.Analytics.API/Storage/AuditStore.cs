using Learnify.Core.Domain;
using Learnify.Analytics.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Analytics.API.Storage;

public interface IAuditStore
{
    Task RecordAsync(AuditEntry entry);
    Task<List<AuditEntry>> GetLatestLogsAsync(int count = 100);
}

public class AuditStore : IAuditStore
{
    private readonly AnalyticsDbContext _db;

    public AuditStore(AnalyticsDbContext db)
    {
        _db = db;
    }

    public async Task RecordAsync(AuditEntry entry)
    {
        _db.AuditEntries.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditEntry>> GetLatestLogsAsync(int count = 100)
    {
        return await _db.AuditEntries
            .OrderByDescending(a => a.OccurredAt)
            .Take(count)
            .ToListAsync();
    }
}
