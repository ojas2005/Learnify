using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Analytics.API.DbContexts;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<LearnerAccount> LearnerAccounts => Set<LearnerAccount>();
    public DbSet<CourseOffering> CourseOfferings => Set<CourseOffering>();
    public DbSet<CourseRegistration> CourseRegistrations => Set<CourseRegistration>();
    public DbSet<CourseFeedback> CourseFeedback => Set<CourseFeedback>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<AuditEntry>(e =>
        {
            e.ToTable("Analytics_AuditLogs");
        });

        mb.Entity<LearnerAccount>(e =>
        {
            e.ToTable("Identity_Accounts");
        });

        mb.Entity<CourseOffering>(e =>
        {
            e.ToTable("Courses_Catalog");
        });

        mb.Entity<CourseRegistration>(e =>
        {
            e.ToTable("Registration_Enrollments");
        });

        mb.Entity<CourseFeedback>(e =>
        {
            e.ToTable("Reviews_Comments");
        });

        // Configure relationships for analytics queries
        mb.Entity<CourseOffering>()
            .HasMany(c => c.Registrations)
            .WithOne(r => r.Course)
            .HasForeignKey(r => r.CourseId);

        mb.Entity<CourseOffering>()
            .HasMany(c => c.FeedbackEntries)
            .WithOne(f => f.Course)
            .HasForeignKey(f => f.CourseId);

        mb.Entity<LearnerAccount>()
            .HasMany(l => l.CourseRegistrations)
            .WithOne(r => r.Learner)
            .HasForeignKey(r => r.LearnerId);

        mb.Entity<LearnerAccount>()
            .HasMany(l => l.FeedbackSubmissions)
            .WithOne(f => f.Learner)
            .HasForeignKey(f => f.LearnerId);

        mb.Entity<CompletionCredential>()
            .HasOne(c => c.Learner)
            .WithMany()
            .HasForeignKey(c => c.LearnerId)
            .OnDelete(DeleteBehavior.NoAction);

        mb.Entity<CompletionCredential>()
            .HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
