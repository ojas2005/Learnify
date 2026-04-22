using Learnify.Core.Core;
using Learnify.Core.Domain;

namespace Learnify.Curriculum.API.Application;

public interface ICurriculumBuilder
{
    Task<OperationResult<CurriculumLesson>> AppendLessonAsync(CurriculumLesson lesson);
    Task<OperationResult<CurriculumLesson>> GetLessonAsync(int lessonId);
    Task<List<CurriculumLesson>> GetCourseCurriculumAsync(int courseId);
    Task<List<CurriculumLesson>> GetPreviewableLessonsAsync(int courseId);
    Task<OperationResult<CurriculumLesson>> AmendLessonAsync(CurriculumLesson lesson);
    Task ResequenceAsync(int courseId, List<int> orderedLessonIds);
    Task<OperationResult> MakeLessonLiveAsync(int lessonId);
    Task<OperationResult> DropLessonAsync(int lessonId);
    Task PurgeCourseCurriculumAsync(int courseId);
    Task<int> CountLessonsAsync(int courseId);
}
