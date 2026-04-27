using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Exams.API.Storage;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Learnify.Exams.API.Application;

// this engine handles exams from creation to scoring
public class ExamEngine : IExamEngine
{
    private readonly IExamStore _examStore;
    private readonly IAttemptStore _attemptStore;
    private readonly ILogger<ExamEngine> _log;

    public ExamEngine(IExamStore examStore, IAttemptStore attemptStore, ILogger<ExamEngine> log)
    {
        _examStore = examStore;
        _attemptStore = attemptStore;
        _log = log;
    }

    public async Task<OperationResult<CourseExam>> CreateExamAsync(CourseExam exam)
    {
        if (string.IsNullOrWhiteSpace(exam.Title))
            return OperationResult<CourseExam>.BusinessRuleViolation("Exam title cannot be blank.");

        if (exam.PassThreshold is < 0 or > 100)
            return OperationResult<CourseExam>.BusinessRuleViolation("Pass threshold must be between 0 and 100.");

        if (exam.AttemptsAllowed < 1)
            return OperationResult<CourseExam>.BusinessRuleViolation("At least one attempt must be allowed.");

        exam.CreatedOn = DateTime.UtcNow;
        exam.IsPublished = false;

        var saved = await _examStore.InsertExamAsync(exam);
        _log.LogInformation("Exam {ExamId} created for course {CourseId}", saved.Id, saved.CourseId);

        return OperationResult<CourseExam>.Ok(saved);
    }

    public async Task<OperationResult<CourseExam>> GetExamAsync(int examId)
    {
        var exam = await _examStore.GetByIdAsync(examId);
        return exam is null
            ? OperationResult<CourseExam>.NotFound($"Exam {examId} not found.")
            : OperationResult<CourseExam>.Ok(exam);
    }

    public async Task<List<CourseExam>> GetExamsForCourseAsync(int courseId)
    {
        return await _examStore.GetByCourseAsync(courseId);
    }

    public async Task<OperationResult<CourseExam>> UpdateExamAsync(CourseExam updates)
    {
        var existing = await _examStore.GetByIdAsync(updates.Id);
        if (existing is null)
            return OperationResult<CourseExam>.NotFound("Exam not found.");

        existing.Title = updates.Title.Trim();
        existing.QuestionsPayload = updates.QuestionsPayload;
        existing.PassThreshold = updates.PassThreshold;
        existing.AttemptsAllowed = updates.AttemptsAllowed;
        existing.LessonId = updates.LessonId;

        var saved = await _examStore.UpdateExamAsync(existing);
        return OperationResult<CourseExam>.Ok(saved);
    }

    public async Task<OperationResult<CourseExam>> PublishExamAsync(int examId)
    {
        var exam = await _examStore.GetByIdAsync(examId);
        if (exam is null)
            return OperationResult<CourseExam>.NotFound("Exam not found.");

        // check if exam is empty before publishing
        if (string.IsNullOrWhiteSpace(exam.QuestionsPayload) || exam.QuestionsPayload == "[]")
            return OperationResult<CourseExam>.BusinessRuleViolation(
                "Cannot publish an exam with no questions.");

        if (exam.IsPublished)
            return OperationResult<CourseExam>.Conflict("Exam is already published.");

        exam.IsPublished = true;
        var saved = await _examStore.UpdateExamAsync(exam);
        _log.LogInformation("Exam {ExamId} published", examId);

        return OperationResult<CourseExam>.Ok(saved);
    }

    public async Task<OperationResult> RemoveExamAsync(int examId)
    {
        var exam = await _examStore.GetByIdAsync(examId);
        if (exam is null)
            return OperationResult.NotFound("Exam not found.");

        await _examStore.DeleteExamAsync(examId);
        return OperationResult.Done;
    }

    // starts a new attempt for a learner and checks the limit
    public async Task<OperationResult<ExamAttempt>> OpenAttemptAsync(int learnerId, int examId)
    {
        var exam = await _examStore.GetByIdAsync(examId);
        if (exam is null)
            return OperationResult<ExamAttempt>.NotFound("Exam not found.");

        if (!exam.IsPublished)
            return OperationResult<ExamAttempt>.BusinessRuleViolation("This exam is not available yet.");

        // check attempt limit
        var previousAttempts = await _attemptStore.CountAttemptsAsync(learnerId, examId);
        if (previousAttempts >= exam.AttemptsAllowed)
        {
            return OperationResult<ExamAttempt>.BusinessRuleViolation(
                $"You have used all {exam.AttemptsAllowed} allowed attempt(s) for this exam.");
        }

        var attempt = new ExamAttempt
        {
            ExamId = examId,
            LearnerId = learnerId,
            BeganAt = DateTime.UtcNow,
            AnswersPayload = string.Empty
        };

        var saved = await _attemptStore.CreateAttemptAsync(attempt);
        _log.LogInformation("Learner {LearnerId} started attempt {AttemptId} on exam {ExamId}",
            learnerId, saved.Id, examId);

        return OperationResult<ExamAttempt>.Ok(saved);
    }

    // submits and grades an attempt on the server side
    public async Task<OperationResult<ExamAttempt>> SubmitAttemptAsync(
        int attemptId, int learnerId, string answersJson)
    {
        var attempt = await _attemptStore.GetByIdAsync(attemptId);
        if (attempt is null)
            return OperationResult<ExamAttempt>.NotFound("Attempt not found.");

        if (attempt.LearnerId != learnerId)
            return OperationResult<ExamAttempt>.AccessDenied("This is not your attempt.");

        if (attempt.SubmittedAt.HasValue)
            return OperationResult<ExamAttempt>.Conflict("This attempt has already been submitted.");

        var exam = await _examStore.GetByIdAsync(attempt.ExamId);
        if (exam is null)
            return OperationResult<ExamAttempt>.NotFound("Parent exam not found.");

        // score is computed here to keep it safe
        var score = TallyScore(exam.QuestionsPayload, answersJson);

        attempt.AnswersPayload = answersJson;
        attempt.Score = score;
        attempt.HasPassed = score >= exam.PassThreshold;
        attempt.SubmittedAt = DateTime.UtcNow;

        var saved = await _attemptStore.UpdateAttemptAsync(attempt);

        _log.LogInformation("Attempt {AttemptId} submitted — score {Score}, passed={Passed}",
            attemptId, score, attempt.HasPassed);

        return OperationResult<ExamAttempt>.Ok(saved);
    }

    public async Task<OperationResult<ExamAttempt>> GetAttemptAsync(int attemptId)
    {
        var attempt = await _attemptStore.GetByIdAsync(attemptId);
        return attempt is null
            ? OperationResult<ExamAttempt>.NotFound($"Attempt {attemptId} not found.")
            : OperationResult<ExamAttempt>.Ok(attempt);
    }

    public async Task<List<ExamAttempt>> GetAttemptsForLearnerAsync(int learnerId, int examId)
    {
        return await _attemptStore.GetByLearnerAndExamAsync(learnerId, examId);
    }

    public async Task<List<ExamAttempt>> GetAllAttemptsForExamAsync(int examId)
    {
        return await _attemptStore.GetAllForExamAsync(examId);
    }

    // scoring logic

    // compares answers against correct ones and returns score from 0 to 100
    private static int TallyScore(string questionsJson, string answersJson)
    {
        try
        {
            var questions = JsonSerializer.Deserialize<List<QuestionRecord>>(questionsJson);
            var answers = JsonSerializer.Deserialize<Dictionary<string, int>>(answersJson);

            if (questions is null || answers is null || questions.Count == 0)
                return 0;

            var correct = questions.Count(q => answers.TryGetValue(q.Id.ToString(), out var given) && given == q.CorrectOption);

            return (int)Math.Round((double)correct / questions.Count * 100);
        }
        catch
        {
            // return zero if json is bad
            return 0;
        }
    }

    // internal types

    private sealed class QuestionRecord
    {
        public int Id { get; set; }
        public int CorrectOption { get; set; }
    }
}
