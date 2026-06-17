using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Curriculum.API.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Learnify.Curriculum.API.Endpoints;

[ApiController]
[Route("api/curriculum")]
[Produces("application/json")]
public class CurriculumController : ControllerBase
{
    private readonly ICurriculumBuilder _builder;

    public CurriculumController(ICurriculumBuilder builder)
    {
        _builder = builder;
    }

    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCurriculum(int courseId)
    {
        return Ok((await _builder.GetCourseCurriculumAsync(courseId)).Select(ToView));
    }

    [HttpGet("course/{courseId:int}/previews")]
    public async Task<IActionResult> GetPreviews(int courseId)
    {
        return Ok((await _builder.GetPreviewableLessonsAsync(courseId)).Select(ToView));
    }

    [HttpGet("lesson/{lessonId:int}")]
    public async Task<IActionResult> GetLesson(int lessonId)
    {
        var result = await _builder.GetLessonAsync(lessonId);
        return result.Succeeded ? Ok(ToView(result.Payload!)) : Fail(result);
    }

    [HttpPost("course/{courseId:int}/lessons")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> AddLesson(int courseId, [FromBody] AddLessonRequest req)
    {
        var lesson = new CurriculumLesson
        {
            CourseId = courseId,
            Title = req.Title,
            Body = req.Body,
            Format = req.Format,
            MediaUrl = req.MediaUrl,
            DurationMinutes = req.DurationMinutes,
            IsPreviewable = req.IsPreviewable
        };

        var result = await _builder.AppendLessonAsync(lesson);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetLesson), new { lessonId = result.Payload!.Id }, ToView(result.Payload))
            : Fail(result);
    }

    [HttpPut("lesson/{lessonId:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonRequest req)
    {
        var lesson = new CurriculumLesson
        {
            Id = lessonId,
            Title = req.Title,
            Body = req.Body,
            Format = req.Format,
            MediaUrl = req.MediaUrl,
            DurationMinutes = req.DurationMinutes,
            IsPreviewable = req.IsPreviewable
        };

        var result = await _builder.AmendLessonAsync(lesson);
        return result.Succeeded ? Ok(ToView(result.Payload!)) : Fail(result);
    }

    [HttpPatch("course/{courseId:int}/resequence")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Resequence(int courseId, [FromBody] List<int> orderedLessonIds)
    {
        await _builder.ResequenceAsync(courseId, orderedLessonIds);
        return NoContent();
    }

    [HttpPost("lesson/{lessonId:int}/publish")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> PublishLesson(int lessonId)
    {
        var result = await _builder.MakeLessonLiveAsync(lessonId);
        return result.Succeeded ? NoContent() : Fail(result);
    }

    [HttpDelete("lesson/{lessonId:int}")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> DeleteLesson(int lessonId)
    {
        var result = await _builder.DropLessonAsync(lessonId);
        return result.Succeeded ? NoContent() : Fail(result);
    }

    private IActionResult Fail<T>(OperationResult<T> r)
    {
        return r.Kind switch
        {
            FailureKind.NotFound => NotFound(new { message = r.FailureReason }),
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

    private static LessonView ToView(CurriculumLesson l)
    {
        return new()
        {
            Id = l.Id,
            CourseId = l.CourseId,
            Title = l.Title,
            Body = l.Body,
            Format = l.Format,
            MediaUrl = l.MediaUrl,
            SequencePosition = l.SequencePosition,
            DurationMinutes = l.DurationMinutes,
            IsPublished = l.IsPublished,
            IsPreviewable = l.IsPreviewable,
            AddedOn = l.AddedOn
        };
    }
}

// inline contracts for this controller
public class AddLessonRequest
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public MediaFormat Format { get; set; }
    [StringLength(500)]
    public string? MediaUrl { get; set; }
    [Range(0, 600)]
    public int DurationMinutes { get; set; }
    public bool IsPreviewable { get; set; }
}

public class UpdateLessonRequest
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public MediaFormat Format { get; set; }
    [StringLength(500)]
    public string? MediaUrl { get; set; }
    [Range(0, 600)]
    public int DurationMinutes { get; set; }
    public bool IsPreviewable { get; set; }
}

public class LessonView
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public MediaFormat Format { get; set; }
    public string? MediaUrl { get; set; }
    public int SequencePosition { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsPublished { get; set; }
    public bool IsPreviewable { get; set; }
    public DateTime AddedOn { get; set; }
}
