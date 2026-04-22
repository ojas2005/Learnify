using Learnify.Core.Domain;

namespace Learnify.Curriculum.API.Storage;

public interface ISyllabusStore
{
    Task<CurriculumLesson?> GetLessonByIdAsync(int lessonId);
    Task<List<CurriculumLesson>> GetLessonsInOrderAsync(int courseId);
    Task<List<CurriculumLesson>> GetPreviewLessonsAsync(int courseId);
    Task<int> LessonCountForCourseAsync(int courseId);
    Task ApplyNewSequenceAsync(int courseId, List<int> orderedIds);
    Task<CurriculumLesson> InsertLessonAsync(CurriculumLesson lesson);
    Task<CurriculumLesson> UpdateLessonAsync(CurriculumLesson lesson);
    Task DeleteLessonAsync(int lessonId);
    Task DeleteAllForCourseAsync(int courseId);
}
