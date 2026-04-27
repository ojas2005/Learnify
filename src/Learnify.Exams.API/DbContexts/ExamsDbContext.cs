using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Exams.API.DbContexts;

public class ExamsDbContext : DbContext
{
    public ExamsDbContext(DbContextOptions<ExamsDbContext> options) : base(options)
    {
    }

    public DbSet<CourseExam> Exams { get { return Set<CourseExam>(); } }
    public DbSet<ExamAttempt> Attempts { get { return Set<ExamAttempt>(); } }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<CourseExam>(e =>
        {
            e.ToTable("CourseExams");
            e.HasKey(ex => ex.Id);
            e.Property(ex => ex.Title).HasMaxLength(200);
            e.HasMany(ex => ex.Attempts).WithOne(a => a.Exam).HasForeignKey(a => a.ExamId);
        });

        mb.Entity<ExamAttempt>(e =>
        {
            e.ToTable("ExamAttempts");
            e.HasKey(a => a.Id);
            // index learner and exam for quick lookups
            e.HasIndex(a => new { a.LearnerId, a.ExamId });
        });
    }
}
