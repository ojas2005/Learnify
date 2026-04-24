using Learnify.Core.Core;
using Learnify.Tracking.API.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Learnify.Tracking.API.Endpoints;

[ApiController]
[Route("api/progress")]
[Produces("application/json")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly ILearningJournal _journal;

    public ProgressController(ILearningJournal journal)
    {
        _journal = journal;
    }

    // records that a learner watched a lesson - can be called multiple times safely
    [HttpPost("watch")]
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> RecordWatch([FromBody] WatchEventRequest req)
    {
        var learnerId = CallerId();
        var result = await _journal.RecordWatchAsync(
            learnerId, req.LessonId, req.CourseId, req.SecondsWatched, req.MarkAsCompleted);

        return result.Succeeded
            ? Ok(ToView(result.Payload!))
            : Fail(result);
    }

    // gets the watch progress for one specific lesson
    [HttpGet("lesson/{lessonId:int}")]
    public async Task<IActionResult> GetLessonProgress(int lessonId)
    {
        var learnerId = CallerId();
        var result = await _journal.GetWatchRecordAsync(learnerId, lessonId);
        return result.Succeeded ? Ok(ToView(result.Payload!)) : Fail(result);
    }

    // returns overall course progress plus a list of all lessons watched
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCourseProgress(int courseId, [FromQuery] int totalLessons)
    {
        var learnerId = CallerId();
        var watchHistory = await _journal.GetCourseWatchHistoryAsync(learnerId, courseId);
        var overallPercent = await _journal.ComputeCourseProgressAsync(learnerId, courseId, totalLessons);

        return Ok(new CourseProgressView
        {
            CourseId = courseId,
            LearnerId = learnerId,
            CompletionPercent = overallPercent,
            LessonsWatched = watchHistory.Count(r => r.IsFinished),
            TotalLessons = totalLessons,
            WatchRecords = watchHistory.Select(ToView).ToList()
        });
    }

    // admin only - gets the full watch history for any learner
    [HttpGet("learner/{learnerId:int}/history")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetFullHistory(int learnerId)
    {
        var records = await _journal.GetAllWatchHistoryAsync(learnerId);
        return Ok(records.Select(ToView));
    }

    // checks whether a learner has fully completed a given lesson
    [HttpGet("lesson/{lessonId:int}/completed")]
    public async Task<IActionResult> CheckLessonCompleted(int lessonId)
    {
        var learnerId = CallerId();
        var finished = await _journal.HasFinishedLessonAsync(learnerId, lessonId);
        return Ok(new { lessonId, learnerId, completed = finished });
    }

    // admin only - wipes all progress for a learner in a specific course
    [HttpDelete("course/{courseId:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> WipeCourseProgress(int courseId, [FromQuery] int learnerId)
    {
        await _journal.WipeProgressForCourseAsync(learnerId, courseId);
        return NoContent();
    }

    // reads the learner's id from the jwt token
    private int CallerId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }

    // maps an operation result failure to the right http error response
    private IActionResult Fail<T>(OperationResult<T> r)
    {
        return r.Kind switch
        {
            FailureKind.NotFound => NotFound(new { message = r.FailureReason }),
            _ => BadRequest(new { message = r.FailureReason })
        };
    }

    // maps a domain watch record to the response shape we send back to clients
    private static WatchRecordView ToView(Core.Domain.LessonWatchRecord r)
    {
        return new()
        {
            Id = r.Id,
            LearnerId = r.LearnerId,
            LessonId = r.LessonId,
            CourseId = r.CourseId,
            IsFinished = r.IsFinished,
            FinishedOn = r.FinishedOn,
            SecondsWatched = r.SecondsWatched,
            LastWatchedOn = r.LastWatchedOn
        };
    }
}

// request body for recording a watch event
public class WatchEventRequest
{
    [Required]
    public int LessonId { get; set; }
    [Required]
    public int CourseId { get; set; }
    [Range(0, int.MaxValue)]
    public int SecondsWatched { get; set; }
    public bool MarkAsCompleted { get; set; }
}

// response shape for a single lesson watch record
public class WatchRecordView
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public bool IsFinished { get; set; }
    public DateTime? FinishedOn { get; set; }
    public int SecondsWatched { get; set; }
    public DateTime LastWatchedOn { get; set; }
}

// response shape for overall course progress
public class CourseProgressView
{
    public int CourseId { get; set; }
    public int LearnerId { get; set; }
    public int CompletionPercent { get; set; }
    public int LessonsWatched { get; set; }
    public int TotalLessons { get; set; }
    public List<WatchRecordView> WatchRecords { get; set; } = new();
}
