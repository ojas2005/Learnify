using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Exams.API.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Learnify.Exams.API.Endpoints;

[ApiController]
[Route("api/exams")]
[Produces("application/json")]
[Authorize]
public class ExamController : ControllerBase
{
    private readonly IExamEngine _engine;

    public ExamController(IExamEngine engine)
    {
        _engine = engine;
    }

    // exam management for instructors and admins

    [HttpGet("{examId:int}")]
    public async Task<IActionResult> GetExam(int examId)
    {
        var result = await _engine.GetExamAsync(examId);
        return result.Succeeded ? Ok(ToExamView(result.Payload!)) : Fail(result);
    }

    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCourseExams(int courseId)
    {
        return Ok((await _engine.GetExamsForCourseAsync(courseId)).Select(ToExamView));
    }

    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamRequest req)
    {
        var exam = new CourseExam
        {
            CourseId = req.CourseId,
            LessonId = req.LessonId,
            Title = req.Title,
            QuestionsPayload = req.QuestionsJson ?? "[]",
            PassThreshold = req.PassThreshold,
            AttemptsAllowed = req.AttemptsAllowed
        };

        var result = await _engine.CreateExamAsync(exam);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetExam), new { examId = result.Payload!.Id }, ToExamView(result.Payload))
            : Fail(result);
    }

    [HttpPut("{examId:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdateExam(int examId, [FromBody] UpdateExamRequest req)
    {
        var updates = new CourseExam
        {
            Id = examId,
            Title = req.Title,
            QuestionsPayload = req.QuestionsJson ?? "[]",
            PassThreshold = req.PassThreshold,
            AttemptsAllowed = req.AttemptsAllowed,
            LessonId = req.LessonId
        };

        var result = await _engine.UpdateExamAsync(updates);
        return result.Succeeded ? Ok(ToExamView(result.Payload!)) : Fail(result);
    }

    [HttpPost("{examId:int}/publish")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Publish(int examId)
    {
        var result = await _engine.PublishExamAsync(examId);
        return result.Succeeded ? Ok(ToExamView(result.Payload!)) : Fail(result);
    }

    [HttpDelete("{examId:int}")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> DeleteExam(int examId)
    {
        var result = await _engine.RemoveExamAsync(examId);
        return result.Succeeded ? NoContent() : Fail(result);
    }

    // attempt endpoints for learners

    [HttpPost("{examId:int}/attempts")]
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> StartAttempt(int examId)
    {
        var result = await _engine.OpenAttemptAsync(CallerId(), examId);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetAttempt),
                new { attemptId = result.Payload!.Id }, ToAttemptView(result.Payload))
            : Fail(result);
    }

    [HttpPost("attempts/{attemptId:int}/submit")]
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> SubmitAttempt(int attemptId, [FromBody] AttemptSubmission submission)
    {
        var result = await _engine.SubmitAttemptAsync(attemptId, CallerId(), submission.AnswersJson);
        return result.Succeeded ? Ok(ToAttemptView(result.Payload!)) : Fail(result);
    }

    [HttpGet("attempts/{attemptId:int}")]
    public async Task<IActionResult> GetAttempt(int attemptId)
    {
        var result = await _engine.GetAttemptAsync(attemptId);
        if (!result.Succeeded) return Fail(result);

        var attempt = result.Payload!;
        // learners can only see their own attempts
        if (User.IsInRole("Learner") && attempt.LearnerId != CallerId())
            return Forbid();

        return Ok(ToAttemptView(attempt));
    }

    [HttpGet("{examId:int}/my-attempts")]
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> GetMyAttempts(int examId)
    {
        var attempts = await _engine.GetAttemptsForLearnerAsync(CallerId(), examId);
        return Ok(attempts.Select(ToAttemptView));
    }

    [HttpGet("{examId:int}/all-attempts")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> GetAllAttempts(int examId)
    {
        return Ok((await _engine.GetAllAttemptsForExamAsync(examId)).Select(ToAttemptView));
    }

    // helper methods

    private int CallerId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }

    private IActionResult Fail<T>(OperationResult<T> r)
    {
        return r.Kind switch
        {
            FailureKind.NotFound => NotFound(new { message = r.FailureReason }),
            FailureKind.Conflict => Conflict(new { message = r.FailureReason }),
            FailureKind.AccessDenied => Forbid(),
            _ => BadRequest(new { message = r.FailureReason })
        };
    }

    private IActionResult Fail(OperationResult r)
    {
        return r.Kind switch
        {
            FailureKind.NotFound => NotFound(new { message = r.FailureReason }),
            _ => BadRequest(new { message = r.FailureReason })
        };
    }

    private static ExamView ToExamView(CourseExam e)
    {
        return new ExamView
        {
            Id = e.Id,
            CourseId = e.CourseId,
            LessonId = e.LessonId,
            Title = e.Title,
            PassThreshold = e.PassThreshold,
            AttemptsAllowed = e.AttemptsAllowed,
            IsPublished = e.IsPublished,
            CreatedOn = e.CreatedOn
        };
    }

    private static AttemptView ToAttemptView(ExamAttempt a)
    {
        return new AttemptView
        {
            Id = a.Id,
            ExamId = a.ExamId,
            LearnerId = a.LearnerId,
            BeganAt = a.BeganAt,
            SubmittedAt = a.SubmittedAt,
            Score = a.Score,
            HasPassed = a.HasPassed
        };
    }
}

// inline contracts

public class CreateExamRequest
{
    [Required] public int CourseId { get; set; }
    public int? LessonId { get; set; }
    [Required, StringLength(200)] public string Title { get; set; } = string.Empty;
    public string? QuestionsJson { get; set; }
    [Range(0, 100)] public int PassThreshold { get; set; } = 70;
    [Range(1, 10)] public int AttemptsAllowed { get; set; } = 3;
}

public class UpdateExamRequest
{
    [Required, StringLength(200)] public string Title { get; set; } = string.Empty;
    public string? QuestionsJson { get; set; }
    public int? LessonId { get; set; }
    [Range(0, 100)] public int PassThreshold { get; set; } = 70;
    [Range(1, 10)] public int AttemptsAllowed { get; set; } = 3;
}

public class AttemptSubmission
{
    [Required] public string AnswersJson { get; set; } = string.Empty;
}

public class ExamView
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int? LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PassThreshold { get; set; }
    public int AttemptsAllowed { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class AttemptView
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public int LearnerId { get; set; }
    public DateTime BeganAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int Score { get; set; }
    public bool HasPassed { get; set; }
}
