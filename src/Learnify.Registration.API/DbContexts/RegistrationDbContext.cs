using Learnify.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Registration.API.DbContexts;

public class RegistrationDbContext : DbContext
{
    public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : base(options)
    {
    }

    public DbSet<CourseRegistration> Registrations
    {
        // the list of all course sign ups
        get
        {
            return Set<CourseRegistration>();
        }
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // set up the database table for registrations
        mb.Entity<CourseRegistration>(e =>
        {
            e.ToTable("CourseRegistrations");
            e.HasKey(r => r.Id);
            // make sure a student cant join the same course twice
            e.HasIndex(r => new { r.LearnerId, r.CourseId }).IsUnique();
            e.Property(r => r.PaymentReference).HasMaxLength(100);
        });
    }
}
