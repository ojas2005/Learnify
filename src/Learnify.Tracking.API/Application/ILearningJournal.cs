using Learnify.Core.Core;
using Learnify.Core.Domain;

namespace Learnify.Tracking.API.Application;

public interface ILearningJournal
{
    Task<OperationResult<LessonWatchRecord>> RecordWatchAsync(
        int learnerId, int lessonId, int courseId, int secondsWatched, bool markCompleted);
    Task<OperationResult<LessonWatchRecord>> GetWatchRecordAsync(int learnerId, int lessonId);
    Task<List<LessonWatchRecord>> GetCourseWatchHistoryAsync(int learnerId, int courseId);
    Task<List<LessonWatchRecord>> GetAllWatchHistoryAsync(int learnerId);
    Task<int> ComputeCourseProgressAsync(int learnerId, int courseId, int totalLessonCount);
    Task<bool> HasFinishedLessonAsync(int learnerId, int lessonId);
    Task WipeProgressForCourseAsync(int learnerId, int courseId);
}
