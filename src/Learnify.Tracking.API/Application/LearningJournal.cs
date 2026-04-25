using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Tracking.API.Storage;
using Microsoft.Extensions.Logging;

namespace Learnify.Tracking.API.Application;

// keeps track of what lessons a learner has watched and how far they've come in a course
public class LearningJournal : ILearningJournal
{
    private readonly IWatchRecordStore _watchStore;
    private readonly ILogger<LearningJournal> _log;

    public LearningJournal(IWatchRecordStore watchStore, ILogger<LearningJournal> log)
    {
        _watchStore = watchStore;
        _log = log;
    }

    // saves or updates a watch entry for a lesson - if the learner already has one, we update it
    public async Task<OperationResult<LessonWatchRecord>> RecordWatchAsync(
        int learnerId, int lessonId, int courseId, int secondsWatched, bool markCompleted)
    {
        if (secondsWatched < 0)
            secondsWatched = 0;

        var existing = await _watchStore.FindExistingRecordAsync(learnerId, lessonId);

        if (existing is not null)
        {
            return await RefreshExistingRecord(existing, secondsWatched, markCompleted);
        }

        return await CreateFreshRecord(learnerId, lessonId, courseId, secondsWatched, markCompleted);
    }

    // fetch the watch record for a specific lesson, returns not found if there's nothing yet
    public async Task<OperationResult<LessonWatchRecord>> GetWatchRecordAsync(int learnerId, int lessonId)
    {
        var record = await _watchStore.FindExistingRecordAsync(learnerId, lessonId);
        return record is null
            ? OperationResult<LessonWatchRecord>.NotFound("No watch record found for this lesson.")
            : OperationResult<LessonWatchRecord>.Ok(record);
    }

    // get all watch records the learner has for a specific course
    public async Task<List<LessonWatchRecord>> GetCourseWatchHistoryAsync(int learnerId, int courseId)
    {
        return await _watchStore.GetAllForCourseAsync(learnerId, courseId);
    }

    // get every watch record across all courses for a learner
    public async Task<List<LessonWatchRecord>> GetAllWatchHistoryAsync(int learnerId)
    {
        return await _watchStore.GetAllForLearnerAsync(learnerId);
    }

    // figures out what percentage of the course is done based on finished lessons
    public async Task<int> ComputeCourseProgressAsync(int learnerId, int courseId, int totalLessonCount)
    {
        if (totalLessonCount <= 0)
            return 0;

        var finishedCount = await _watchStore.CountFinishedLessonsAsync(learnerId, courseId);

        // using integer math to get a clean 0-100 number without decimals
        var percent = (int)Math.Round((double)finishedCount / totalLessonCount * 100);
        return Math.Clamp(percent, 0, 100);
    }

    // checks if the learner has fully finished a specific lesson
    public async Task<bool> HasFinishedLessonAsync(int learnerId, int lessonId)
    {
        return await _watchStore.IsLessonFinishedAsync(learnerId, lessonId);
    }

    // deletes all watch records for a course - used when resetting progress
    public async Task WipeProgressForCourseAsync(int learnerId, int courseId)
    {
        await _watchStore.DeleteAllForCourseAsync(learnerId, courseId);
        _log.LogInformation("Progress wiped for learner {LearnerId} in course {CourseId}",
            learnerId, courseId);
    }

    // updates an existing record - keeps the highest seconds and won't un-complete a finished lesson
    private async Task<OperationResult<LessonWatchRecord>> RefreshExistingRecord(
        LessonWatchRecord record, int secondsWatched, bool markCompleted)
    {
        // only move progress forward, never backwards
        if (secondsWatched > record.SecondsWatched)
            record.SecondsWatched = secondsWatched;

        record.LastWatchedOn = DateTime.UtcNow;

        // once a lesson is marked done, it stays done even if rewatched
        if (markCompleted && !record.IsFinished)
        {
            record.IsFinished = true;
            record.FinishedOn = DateTime.UtcNow;
        }

        var updated = await _watchStore.UpdateRecordAsync(record);
        return OperationResult<LessonWatchRecord>.Ok(updated);
    }

    // builds a brand new watch record for a lesson the learner is watching for the first time
    private async Task<OperationResult<LessonWatchRecord>> CreateFreshRecord(
        int learnerId, int lessonId, int courseId, int secondsWatched, bool markCompleted)
    {
        var record = new LessonWatchRecord
        {
            LearnerId = learnerId,
            LessonId = lessonId,
            CourseId = courseId,
            SecondsWatched = secondsWatched,
            IsFinished = markCompleted,
            FinishedOn = markCompleted ? DateTime.UtcNow : null,
            LastWatchedOn = DateTime.UtcNow
        };

        var saved = await _watchStore.InsertRecordAsync(record);

        _log.LogDebug("Watch record created for learner {LearnerId}, lesson {LessonId}", learnerId, lessonId);

        return OperationResult<LessonWatchRecord>.Ok(saved);
    }
}
