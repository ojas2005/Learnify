using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Core.Enums;

namespace Learnify.Registration.API.Application;

public interface ISeatReservation
{
    Task<OperationResult<CourseRegistration>> ClaimSeatAsync(int learnerId, int courseId);
    Task<OperationResult<CourseRegistration>> GetRegistrationAsync(int registrationId);
    Task<List<CourseRegistration>> GetLearnerRegistrationsAsync(int learnerId);
    Task<List<CourseRegistration>> GetCourseRosterAsync(int courseId);
    Task<bool> IsRegisteredAsync(int learnerId, int courseId);
    Task<OperationResult> RecordProgressAsync(int registrationId, int completionPercent);
    Task<OperationResult> MarkFinishedAsync(int registrationId);
    Task<OperationResult> WithdrawAsync(int registrationId);
    Task<OperationResult> GrantCredentialAsync(int registrationId);
    Task<List<CourseRegistration>> GetCompletedAsync(int learnerId);
    Task<List<CourseRegistration>> GetActiveAsync(int learnerId);
    Task<int> HeadcountForCourseAsync(int courseId);
}
