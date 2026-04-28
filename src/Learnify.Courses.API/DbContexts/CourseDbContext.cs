using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Courses.API.DbContexts;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options) { }

    public DbSet<CourseOffering> Courses => Set<CourseOffering>();
    public DbSet<LearnerAccount> Learners => Set<LearnerAccount>();
    public DbSet<CurriculumLesson> Lessons => Set<CurriculumLesson>();
    public DbSet<CourseFeedback> FeedbackEntries => Set<CourseFeedback>();
    public DbSet<CourseRegistration> CourseRegistrations => Set<CourseRegistration>();
    public DbSet<CourseExam> CourseExams => Set<CourseExam>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<CourseOffering>(e =>
        {
            e.ToTable("CourseOfferings");
            e.HasKey(c => c.Id);
            e.Property(c => c.ListPrice).HasColumnType("decimal(10,2)");
            e.HasOne(c => c.Author).WithMany(a => a.AuthoredCourses).HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<LearnerAccount>(e =>
        {
            e.ToTable("LearnerAccounts");
            e.HasKey(l => l.Id);
        });

        mb.Entity<CurriculumLesson>(e =>
        {
            e.ToTable("CurriculumLessons");
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Course).WithMany(c => c.Lessons).HasForeignKey(l => l.CourseId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<CourseFeedback>(e =>
        {
            e.ToTable("CourseFeedback");
            e.HasKey(f => f.Id);
            e.HasOne(f => f.Course).WithMany(c => c.FeedbackEntries).HasForeignKey(f => f.CourseId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<CourseRegistration>(e =>
        {
            e.ToTable("CourseRegistrations");
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Course).WithMany(c => c.Registrations).HasForeignKey(r => r.CourseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Learner).WithMany(l => l.CourseRegistrations).HasForeignKey(r => r.LearnerId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<CourseExam>(e =>
        {
            e.ToTable("CourseExams");
            e.HasKey(ex => ex.Id);
            e.HasOne(ex => ex.Course).WithMany(c => c.Exams).HasForeignKey(ex => ex.CourseId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
