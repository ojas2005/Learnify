using Learnify.Core.Core;
using Learnify.Core.Domain;

namespace Learnify.Exams.API.Application;

public interface IExamEngine
{
    Task<OperationResult<CourseExam>> CreateExamAsync(CourseExam exam);
    Task<OperationResult<CourseExam>> GetExamAsync(int examId);
    Task<List<CourseExam>> GetExamsForCourseAsync(int courseId);
    Task<OperationResult<CourseExam>> UpdateExamAsync(CourseExam updates);
    Task<OperationResult<CourseExam>> PublishExamAsync(int examId);
    Task<OperationResult> RemoveExamAsync(int examId);
    Task<OperationResult<ExamAttempt>> OpenAttemptAsync(int learnerId, int examId);
    Task<OperationResult<ExamAttempt>> SubmitAttemptAsync(int attemptId, int learnerId, string answersJson);
    Task<OperationResult<ExamAttempt>> GetAttemptAsync(int attemptId);
    Task<List<ExamAttempt>> GetAttemptsForLearnerAsync(int learnerId, int examId);
    Task<List<ExamAttempt>> GetAllAttemptsForExamAsync(int examId);
}
