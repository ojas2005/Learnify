using Learnify.Core.Domain;

namespace Learnify.Courses.API.Storage;

public interface ICourseStore
{
    Task<CourseOffering?> GetByIdAsync(int courseId);
    Task<List<CourseOffering>> GetByAuthorAsync(int authorId);
    Task<List<CourseOffering>> GetByTopicAsync(string topic);
    Task<List<CourseOffering>> GetPublishedAndApprovedAsync();
    Task<List<CourseOffering>> FullTextSearchAsync(string terms);
    Task<List<CourseOffering>> GetTopRatedAsync(int limit);
    Task IncrementRegistrationCountAsync(int courseId);
    Task<CourseOffering> AddCourseAsync(CourseOffering course);
    Task<CourseOffering> UpdateCourseAsync(CourseOffering course);
    Task RemoveCourseAsync(int courseId);
}
