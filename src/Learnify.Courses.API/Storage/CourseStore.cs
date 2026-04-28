using Learnify.Core.Domain;
using Learnify.Courses.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Courses.API.Storage;

public class CourseStore : ICourseStore
{
    private readonly CourseDbContext _db;

    public CourseStore(CourseDbContext db)
    {
        _db = db;
    }

    public async Task<CourseOffering?> GetByIdAsync(int courseId)
    {
        return await _db.Courses
            .Include(c => c.Author)
            .Include(c => c.Lessons)
            .Include(c => c.FeedbackEntries)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }

    public async Task<List<CourseOffering>> GetByAuthorAsync(int authorId)
    {
        return await _db.Courses.Include(c => c.Author).Where(c => c.AuthorId == authorId).OrderByDescending(c => c.CreatedOn).ToListAsync();
    }

    public async Task<List<CourseOffering>> GetByTopicAsync(string topic)
    {
        return await _db.Courses.Include(c => c.Author).Include(c => c.FeedbackEntries).Where(c => c.Topic == topic && c.IsPublished && c.IsApprovedByAdmin).ToListAsync();
    }

    public async Task<List<CourseOffering>> GetPublishedAndApprovedAsync()
    {
        return await _db.Courses.Include(c => c.Author).Include(c => c.FeedbackEntries).Where(c => c.IsPublished && c.IsApprovedByAdmin).OrderByDescending(c => c.CreatedOn).ToListAsync();
    }

    public async Task<List<CourseOffering>> FullTextSearchAsync(string terms)
    {
        return await _db.Courses.Include(c => c.Author).Include(c => c.FeedbackEntries).Where(c => c.IsPublished && c.IsApprovedByAdmin && (c.Title.Contains(terms) || (c.Synopsis != null && c.Synopsis.Contains(terms)))).ToListAsync();
    }

    public async Task<List<CourseOffering>> GetTopRatedAsync(int limit)
    {
        return await _db.Courses.Include(c => c.Author).Include(c => c.FeedbackEntries).Where(c => c.IsPublished && c.IsApprovedByAdmin && c.FeedbackEntries.Any(f => f.IsApproved))
            .Select(c => new
            {
                Course = c,
                AvgRating = c.FeedbackEntries.Where(f => f.IsApproved).Average(f => (double?)f.StarRating) ?? 0
            }).OrderByDescending(x => x.AvgRating).Take(limit).Select(x => x.Course).ToListAsync();
    }

    public async Task IncrementRegistrationCountAsync(int courseId)
    {
        await _db.Courses.Where(c => c.Id == courseId).ExecuteUpdateAsync(s => s.SetProperty(c => c.TotalRegistrations, c => c.TotalRegistrations + 1));
    }

    public async Task<CourseOffering> AddCourseAsync(CourseOffering course)
    {
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return course;
    }

    public async Task<CourseOffering> UpdateCourseAsync(CourseOffering course)
    {
        _db.Courses.Update(course);
        await _db.SaveChangesAsync();
        return course;
    }

    public async Task RemoveCourseAsync(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course is not null)
        {
            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<LearnerAccount?> GetAuthorByIdAsync(int authorId)
    {
        return await _db.Learners.FindAsync(authorId);
    }

    public async Task<LearnerAccount> AddAuthorAsync(LearnerAccount author)
    {
        _db.Learners.Add(author);
        await _db.SaveChangesAsync();
        return author;
    }

    public async Task AddAuthorWithIdAsync(int authorId, string displayName, string email)
    {
        var sql = @"
            SET IDENTITY_INSERT LearnerAccounts ON;
            INSERT INTO LearnerAccounts (Id, DisplayName, EmailAddress, HashedPassword, Role, ProfilePictureUrl, IsActive, RegisteredOn)
            VALUES ({0}, {1}, {2}, 'synced_from_identity', 1, NULL, 1, GETDATE());
            SET IDENTITY_INSERT LearnerAccounts OFF;
        ";
        
        await _db.Database.ExecuteSqlRawAsync(sql, authorId, displayName, email);
    }
}
