using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Courses.API.DbContexts;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options) { }

    public DbSet<CourseOffering> Courses
    {
        get
        {
            return Set<CourseOffering>();
        }
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<CourseOffering>(e =>
        {
            e.ToTable("CourseOfferings");
            e.HasKey(c => c.Id);
            e.Property(c => c.ListPrice).HasColumnType("decimal(10,2)");
            e.HasOne(c => c.Author).WithMany(a => a.AuthoredCourses).HasForeignKey(c => c.AuthorId);
        });
    }
}
