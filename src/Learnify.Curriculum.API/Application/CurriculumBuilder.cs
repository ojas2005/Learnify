using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Curriculum.API.Storage;
using Microsoft.Extensions.Logging;

namespace Learnify.Curriculum.API.Application;

// manages the ordered list of lessons that make up a course curriculum.
// sequence positioning is handled automatically so callers dont need to track order numbers.
public class CurriculumBuilder : ICurriculumBuilder
{
    private readonly ISyllabusStore _store;
    private readonly ILogger<CurriculumBuilder> _log;

    public CurriculumBuilder(ISyllabusStore store, ILogger<CurriculumBuilder> log)
    {
        _store = store;
        _log = log;
    }

    public async Task<OperationResult<CurriculumLesson>> AppendLessonAsync(CurriculumLesson lesson)
    {
        if (string.IsNullOrWhiteSpace(lesson.Title))
            return OperationResult<CurriculumLesson>.BusinessRuleViolation("Lesson title cannot be blank.");

        // auto assign sequence position at the end of existing curriculum
        var tailPosition = await DetermineNextPositionAsync(lesson.CourseId);
        lesson.SequencePosition = tailPosition;
        lesson.AddedOn = DateTime.UtcNow;
        lesson.IsPublished = false; // always starts as draft

        var saved = await _store.InsertLessonAsync(lesson);
        _log.LogInformation("Lesson {LessonId} added to course {CourseId} at position {Pos}",
            saved.Id, saved.CourseId, saved.SequencePosition);

        return OperationResult<CurriculumLesson>.Ok(saved);
    }

    public async Task<OperationResult<CurriculumLesson>> GetLessonAsync(int lessonId)
    {
        var lesson = await _store.GetLessonByIdAsync(lessonId);
        return lesson is null
            ? OperationResult<CurriculumLesson>.NotFound($"Lesson {lessonId} does not exist.")
            : OperationResult<CurriculumLesson>.Ok(lesson);
    }

    public async Task<List<CurriculumLesson>> GetCourseCurriculumAsync(int courseId) 
    {
        return await _store.GetLessonsInOrderAsync(courseId);
    }

    public async Task<List<CurriculumLesson>> GetPreviewableLessonsAsync(int courseId)
    {
        return await _store.GetPreviewLessonsAsync(courseId);
    }

    public async Task<OperationResult<CurriculumLesson>> AmendLessonAsync(CurriculumLesson lesson)
    {
        var existing = await _store.GetLessonByIdAsync(lesson.Id);
        if (existing is null)
            return OperationResult<CurriculumLesson>.NotFound("Lesson not found.");

        existing.Title = lesson.Title.Trim();
        existing.Body = lesson.Body;
        existing.Format = lesson.Format;
        existing.MediaUrl = lesson.MediaUrl;
        existing.DurationMinutes = lesson.DurationMinutes;
        existing.IsPreviewable = lesson.IsPreviewable;

        var updated = await _store.UpdateLessonAsync(existing);
        return OperationResult<CurriculumLesson>.Ok(updated);
    }

    public async Task ResequenceAsync(int courseId, List<int> orderedLessonIds)
    {
        // validate all ids belong to this course before applying reorder
        var lessons = await _store.GetLessonsInOrderAsync(courseId);
        var lessonIds = lessons.Select(l => l.Id).ToHashSet();

        var orphaned = orderedLessonIds.Where(id => !lessonIds.Contains(id)).ToList();
        if (orphaned.Any())
        {
            _log.LogWarning("Resequence skipped — IDs {Ids} don't belong to course {CourseId}",
                string.Join(",", orphaned), courseId);
            return;
        }

        await _store.ApplyNewSequenceAsync(courseId, orderedLessonIds);
    }

    public async Task<OperationResult> MakeLessonLiveAsync(int lessonId)
    {
        var lesson = await _store.GetLessonByIdAsync(lessonId);
        if (lesson is null)
            return OperationResult.NotFound("Lesson not found.");

        if (string.IsNullOrWhiteSpace(lesson.MediaUrl) && lesson.Body == null)
            return OperationResult.BusinessRuleViolation(
                "Lesson must have either a media URL or body content before publishing.");

        lesson.IsPublished = true;
        await _store.UpdateLessonAsync(lesson);

        return OperationResult.Done;
    }

    public async Task<OperationResult> DropLessonAsync(int lessonId)
    {
        var lesson = await _store.GetLessonByIdAsync(lessonId);
        if (lesson is null)
            return OperationResult.NotFound("Lesson not found.");

        await _store.DeleteLessonAsync(lessonId);

        // resequence remaining lessons to close the gap
        var remaining = await _store.GetLessonsInOrderAsync(lesson.CourseId);
        var reordered = remaining.Select(l => l.Id).ToList();
        await _store.ApplyNewSequenceAsync(lesson.CourseId, reordered);

        return OperationResult.Done;
    }

    public async Task PurgeCourseCurriculumAsync(int courseId)
    {
        await _store.DeleteAllForCourseAsync(courseId);
    }

    public async Task<int> CountLessonsAsync(int courseId)
    {
        return await _store.LessonCountForCourseAsync(courseId);
    }

    private async Task<int> DetermineNextPositionAsync(int courseId)
    {
        var existing = await _store.GetLessonsInOrderAsync(courseId);
        return existing.Any() ? existing.Max(l => l.SequencePosition) + 1 : 1;
    }
}
