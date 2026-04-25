using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Tracking.API.DbContexts;

// database context for the tracking service - manages the watch records table
public class TrackingDbContext : DbContext
{
    public TrackingDbContext(DbContextOptions<TrackingDbContext> options) : base(options) { }

    public DbSet<LessonWatchRecord> WatchRecords => Set<LessonWatchRecord>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<LessonWatchRecord>(e =>
        {
            e.ToTable("LessonWatchRecords");
            e.HasKey(r => r.Id);
            // this index covers the most common query - did this learner watch this lesson?
            e.HasIndex(r => new { r.LearnerId, r.LessonId }).IsUnique();
            e.HasIndex(r => new { r.LearnerId, r.CourseId });
        });
    }
}
