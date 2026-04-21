using Learnify.Core.Core;
using Learnify.Core.Domain;

namespace Learnify.Courses.API.Application;

public interface ICourseCatalog
{
    Task<OperationResult<CourseOffering>> ListNewCourseAsync(CourseOffering draft, int authorId);
    Task<OperationResult<CourseOffering>> GetCourseAsync(int courseId);
    Task<List<CourseOffering>> GetByAuthorAsync(int authorId);
    Task<List<CourseOffering>> GetByTopicAsync(string topic);
    Task<List<CourseOffering>> BrowseLiveCatalogAsync();
    Task<List<CourseOffering>> SearchAsync(string terms);
    Task<List<CourseOffering>> GetHighlyRatedAsync(int limit);
    Task<OperationResult<CourseOffering>> ReviseDetailsAsync(int courseId, int requestingAuthorId, CourseOffering updates);
    Task<OperationResult<CourseOffering>> SubmitForReviewAsync(int courseId, int requestingAuthorId);
    Task<OperationResult<CourseOffering>> ApproveForLiveAsync(int courseId);
    Task<OperationResult> RemoveCourseAsync(int courseId, int requestingUserId, bool isAdmin);
    Task BumpRegistrationCountAsync(int courseId);
}
