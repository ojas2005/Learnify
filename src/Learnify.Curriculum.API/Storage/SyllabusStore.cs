using Learnify.Core.Domain;
using Learnify.Curriculum.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Curriculum.API.Storage;

public class SyllabusStore : ISyllabusStore
{
    private readonly CurriculumDbContext _db;

    public SyllabusStore(CurriculumDbContext db)
    {
        _db = db;
    }

    public async Task<CurriculumLesson?> GetLessonByIdAsync(int lessonId)
    {
        return await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
    }

    public async Task<List<CurriculumLesson>> GetLessonsInOrderAsync(int courseId)
    {
        return await _db.Lessons.Where(l => l.CourseId == courseId).OrderBy(l => l.SequencePosition).ToListAsync();
    }

    public async Task<List<CurriculumLesson>> GetPreviewLessonsAsync(int courseId)
    {
        return await _db.Lessons.Where(l => l.CourseId == courseId && l.IsPreviewable && l.IsPublished).OrderBy(l => l.SequencePosition).ToListAsync();
    }

    public async Task<int> LessonCountForCourseAsync(int courseId)
    {
        return await _db.Lessons.CountAsync(l => l.CourseId == courseId);
    }

    public async Task ApplyNewSequenceAsync(int courseId, List<int> orderedIds)
    {
        var lessons = await _db.Lessons.Where(l => l.CourseId == courseId).ToListAsync();

        for (int i = 0; i < orderedIds.Count; i++)
        {
            var lesson = lessons.FirstOrDefault(l => l.Id == orderedIds[i]);
            if (lesson is not null)
                lesson.SequencePosition = i + 1;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<CurriculumLesson> InsertLessonAsync(CurriculumLesson lesson)
    {
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();
        return lesson;
    }

    public async Task<CurriculumLesson> UpdateLessonAsync(CurriculumLesson lesson)
    {
        _db.Lessons.Update(lesson);
        await _db.SaveChangesAsync();
        return lesson;
    }

    public async Task DeleteLessonAsync(int lessonId)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if (lesson is not null)
        {
            _db.Lessons.Remove(lesson);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteAllForCourseAsync(int courseId)
    {
        await _db.Lessons.Where(l => l.CourseId == courseId).ExecuteDeleteAsync();
    }
}
