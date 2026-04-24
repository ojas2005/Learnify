using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Registration.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Registration.API.Storage;

public interface ISeatStore
{
    Task<CourseRegistration?> GetByIdAsync(int registrationId);
    Task<List<CourseRegistration>> GetByLearnerAsync(int learnerId);
    Task<List<CourseRegistration>> GetByCourseAsync(int courseId);
    Task<List<CourseRegistration>> GetByStatusAsync(int learnerId, RegistrationStatus status);
    Task<bool> AlreadyRegisteredAsync(int learnerId, int courseId);
    Task<int> CountByCourseAsync(int courseId);
    Task<CourseRegistration> CreateRegistrationAsync(CourseRegistration registration);
    Task<CourseRegistration> UpdateRegistrationAsync(CourseRegistration registration);
}

public class SeatStore : ISeatStore
{
    private readonly RegistrationDbContext _db;

    public SeatStore(RegistrationDbContext db)
    {
        _db = db;
    }

    public async Task<CourseRegistration?> GetByIdAsync(int id)
    {
        // get a registration by its id including user and course details
        return await _db.Registrations.Include(r => r.Learner).Include(r => r.Course).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<CourseRegistration>> GetByLearnerAsync(int learnerId)
    {
        // get all registrations for a specific student
        return await _db.Registrations.Include(r => r.Learner).Include(r => r.Course).Where(r => r.LearnerId == learnerId).ToListAsync();
    }

    public async Task<List<CourseRegistration>> GetByCourseAsync(int courseId)
    {
        // get all students registered for a specific course
        return await _db.Registrations.Include(r => r.Learner).Include(r => r.Course).Where(r => r.CourseId == courseId).ToListAsync();
    }

    public async Task<List<CourseRegistration>> GetByStatusAsync(int learnerId, RegistrationStatus status)
    {
        // filter registrations by state like active or finished
        return await _db.Registrations.Include(r => r.Learner).Include(r => r.Course).Where(r => r.LearnerId == learnerId && r.Status == status).ToListAsync();
    }

    public async Task<bool> AlreadyRegisteredAsync(int learnerId, int courseId)
    {
        // check if student is already in this course
        return await _db.Registrations.AnyAsync(r => r.LearnerId == learnerId && r.CourseId == courseId);
    }

    public async Task<int> CountByCourseAsync(int courseId)
    {
        // count how many students are in a course
        return await _db.Registrations.CountAsync(r => r.CourseId == courseId);
    }

    public async Task<CourseRegistration> CreateRegistrationAsync(CourseRegistration reg)
    {
        // save new registration info
        _db.Registrations.Add(reg);
        await _db.SaveChangesAsync();
        return reg;
    }

    public async Task<CourseRegistration> UpdateRegistrationAsync(CourseRegistration reg)
    {
        // update existing registration info
        _db.Registrations.Update(reg);
        await _db.SaveChangesAsync();
        return reg;
    }
}
