using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Curriculum.API.DbContexts;

public class CurriculumDbContext : DbContext
{
    public CurriculumDbContext(DbContextOptions<CurriculumDbContext> options) : base(options) { }

    public DbSet<CurriculumLesson> Lessons
    {
        get { return Set<CurriculumLesson>(); }
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<CurriculumLesson>(e =>
        {
            e.ToTable("Curriculum_Lessons");
            e.HasKey(l => l.Id);
            e.Property(l => l.Title).HasMaxLength(200);
            e.HasIndex(l => new { l.CourseId, l.SequencePosition });

            // Cross-service navigation - don't create FKs in this DB
            e.Ignore(l => l.Course);
            e.Ignore(l => l.WatchRecords);
        });
    }
}
