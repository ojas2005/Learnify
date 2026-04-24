using Learnify.Core.Domain;
using Learnify.Tracking.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Tracking.API.Storage;

// defines what operations the store needs to support
public interface IWatchRecordStore
{
    Task<LessonWatchRecord?> FindExistingRecordAsync(int learnerId, int lessonId);
    Task<List<LessonWatchRecord>> GetAllForCourseAsync(int learnerId, int courseId);
    Task<List<LessonWatchRecord>> GetAllForLearnerAsync(int learnerId);
    Task<int> CountFinishedLessonsAsync(int learnerId, int courseId);
    Task<bool> IsLessonFinishedAsync(int learnerId, int lessonId);
    Task<LessonWatchRecord> InsertRecordAsync(LessonWatchRecord record);
    Task<LessonWatchRecord> UpdateRecordAsync(LessonWatchRecord record);
    Task DeleteAllForCourseAsync(int learnerId, int courseId);
}

// handles all database reads and writes for watch records
public class WatchRecordStore : IWatchRecordStore
{
    private readonly TrackingDbContext _db;

    public WatchRecordStore(TrackingDbContext db)
    {
        _db = db;
    }

    // looks for an existing record where both learner and lesson match
    public async Task<LessonWatchRecord?> FindExistingRecordAsync(int learnerId, int lessonId)
    {
        return await _db.WatchRecords.FirstOrDefaultAsync(r => r.LearnerId == learnerId && r.LessonId == lessonId);
    }

    // gets all lessons the learner has watched in a course, sorted by lesson order
    public async Task<List<LessonWatchRecord>> GetAllForCourseAsync(int learnerId, int courseId)
    {
        return await _db.WatchRecords.Where(r => r.LearnerId == learnerId && r.CourseId == courseId).OrderBy(r => r.LessonId).ToListAsync();
    }

    // gets every watch record for a learner across all courses, newest first
    public async Task<List<LessonWatchRecord>> GetAllForLearnerAsync(int learnerId)
    {
        return await _db.WatchRecords.Where(r => r.LearnerId == learnerId).OrderByDescending(r => r.LastWatchedOn).ToListAsync();
    }

    // counts how many lessons in a course the learner has marked as finished
    public async Task<int> CountFinishedLessonsAsync(int learnerId, int courseId)
    {
        return await _db.WatchRecords.CountAsync(r => r.LearnerId == learnerId && r.CourseId == courseId && r.IsFinished);
    }

    // checks if a specific lesson has been finished by the learner
    public async Task<bool> IsLessonFinishedAsync(int learnerId, int lessonId)
    {
        return await _db.WatchRecords.AnyAsync(r => r.LearnerId == learnerId && r.LessonId == lessonId && r.IsFinished);
    }

    // saves a new watch record to the database
    public async Task<LessonWatchRecord> InsertRecordAsync(LessonWatchRecord record)
    {
        _db.WatchRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    // saves changes to an existing watch record
    public async Task<LessonWatchRecord> UpdateRecordAsync(LessonWatchRecord record)
    {
        _db.WatchRecords.Update(record);
        await _db.SaveChangesAsync();
        return record;
    }

    // removes all watch records for a learner in a given course
    public async Task DeleteAllForCourseAsync(int learnerId, int courseId)
    {
        await _db.WatchRecords.Where(r => r.LearnerId == learnerId && r.CourseId == courseId).ExecuteDeleteAsync();
    }
}
