using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Reviews.API.DbContexts;

public class ReviewsDbContext : DbContext
{
    public ReviewsDbContext(DbContextOptions<ReviewsDbContext> options) : base(options) { }

    public DbSet<CourseFeedback> CourseFeedback => Set<CourseFeedback>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<CourseFeedback>(e =>
        {
            e.ToTable("CourseFeedback");
            e.HasKey(f => f.Id);
            e.Property(f => f.ReviewText).HasMaxLength(2000);
            e.HasOne(f => f.Learner)
                .WithMany()
                .HasForeignKey(f => f.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Course)
                .WithMany()
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
