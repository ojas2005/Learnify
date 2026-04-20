using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Identity.API.DbContexts;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<LearnerAccount> Accounts => Set<LearnerAccount>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<LearnerAccount>(e =>
        {
            e.ToTable("LearnerAccounts");
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.EmailAddress).IsUnique();
            e.Property(a => a.EmailAddress).HasMaxLength(255);
            e.Property(a => a.DisplayName).HasMaxLength(100);
        });

        //resolve SQL Server multiple cascade path issues by disabling cascade on certain relationships
        mb.Entity<CompletionCredential>()
            .HasOne(c => c.Learner)
            .WithMany(l => l.EarnedCredentials)
            .OnDelete(DeleteBehavior.NoAction);

        mb.Entity<CourseRegistration>()
            .HasOne(r => r.Learner)
            .WithMany(l => l.CourseRegistrations)
            .OnDelete(DeleteBehavior.NoAction);

        mb.Entity<ExamAttempt>()
            .HasOne(a => a.Learner)
            .WithMany(l => l.ExamAttempts)
            .OnDelete(DeleteBehavior.NoAction);

        mb.Entity<CourseFeedback>()
            .HasOne(f => f.Learner)
            .WithMany(l => l.FeedbackSubmissions)
            .OnDelete(DeleteBehavior.NoAction);

        mb.Entity<LessonWatchRecord>()
            .HasOne(w => w.Learner)
            .WithMany(l => l.WatchHistory)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
