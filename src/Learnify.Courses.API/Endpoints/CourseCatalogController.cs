using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Courses.API.Application;
using Learnify.Courses.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Learnify.Courses.API.Endpoints;

[ApiController]
[Route("api/courses")]
[Produces("application/json")]
public class CourseCatalogController : ControllerBase
{
    private readonly ICourseCatalog _catalog;

    public CourseCatalogController(ICourseCatalog catalog)
    {
        _catalog = catalog;
    }

    // public browsing

    [HttpGet]
    public async Task<IActionResult> BrowseCatalog()
    {
        return Ok((await _catalog.BrowseLiveCatalogAsync()).Select(ToCourseView));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        return Ok((await _catalog.SearchAsync(q ?? string.Empty)).Select(ToCourseView));
    }

    [HttpGet("top-rated")]
    public async Task<IActionResult> TopRated([FromQuery] int count = 10)
    {
        return Ok((await _catalog.GetHighlyRatedAsync(count)).Select(ToCourseView));
    }

    [HttpGet("by-topic/{topic}")]
    public async Task<IActionResult> ByTopic(string topic)
    {
        return Ok((await _catalog.GetByTopicAsync(topic)).Select(ToCourseView));
    }

    [HttpGet("{courseId:int}")]
    public async Task<IActionResult> GetCourse(int courseId)
    {
        var result = await _catalog.GetCourseAsync(courseId);
        return result.Succeeded ? Ok(ToCourseView(result.Payload!)) : Fail(result);
    }

    // instructor endpoints

    [HttpGet("my-courses")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetMyCourses()
    {
        var authorId = CallerId();
        var courses = await _catalog.GetByAuthorAsync(authorId);
        return Ok(courses.Select(ToCourseView));
    }

    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest req)
    {
        var draft = new CourseOffering
        {
            Title = req.Title,
            Synopsis = req.Synopsis,
            Topic = req.Topic,
            Difficulty = req.Difficulty,
            Language = req.Language,
            ListPrice = req.ListPrice,
            CoverImageUrl = req.CoverImageUrl
        };

        var result = await _catalog.ListNewCourseAsync(draft, CallerId());
        return result.Succeeded
            ? CreatedAtAction(nameof(GetCourse), new { courseId = result.Payload!.Id }, ToCourseView(result.Payload))
            : Fail(result);
    }

    [HttpPut("{courseId:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] UpdateCourseRequest req)
    {
        var updates = new CourseOffering
        {
            Title = req.Title,
            Synopsis = req.Synopsis,
            Topic = req.Topic,
            Difficulty = req.Difficulty,
            Language = req.Language,
            ListPrice = req.ListPrice,
            CoverImageUrl = req.CoverImageUrl
        };

        var result = await _catalog.ReviseDetailsAsync(courseId, CallerId(), updates);
        return result.Succeeded ? Ok(ToCourseView(result.Payload!)) : Fail(result);
    }

    [HttpPost("{courseId:int}/submit-for-review")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> SubmitForReview(int courseId)
    {
        var result = await _catalog.SubmitForReviewAsync(courseId, CallerId());
        return result.Succeeded ? Ok(ToCourseView(result.Payload!)) : Fail(result);
    }

    // admin endpoints

    [HttpPost("{courseId:int}/approve")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Approve(int courseId)
    {
        var result = await _catalog.ApproveForLiveAsync(courseId);
        return result.Succeeded ? Ok(ToCourseView(result.Payload!)) : Fail(result);
    }

    [HttpPost("{courseId:int}/reject")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Reject(int courseId)
    {
        var result = await _catalog.RejectCourseAsync(courseId);
        return result.Succeeded ? Ok(ToCourseView(result.Payload!)) : Fail(result);
    }

    [HttpDelete("{courseId:int}")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> DeleteCourse(int courseId)
    {
        var isAdmin = User.IsInRole("Administrator");
        var result = await _catalog.RemoveCourseAsync(courseId, CallerId(), isAdmin);
        return result.Succeeded ? NoContent() : Fail(result);
    }

    // helpers

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
            FailureKind.Conflict => Conflict(new { message = r.FailureReason }),
            FailureKind.AccessDenied => Forbid(),
            _ => BadRequest(new { message = r.FailureReason })
        };
    }

    private static CourseView ToCourseView(CourseOffering c)
    {
        return new()
        {
            Id = c.Id,
            Title = c.Title,
            Synopsis = c.Synopsis,
            AuthorId = c.AuthorId,
            AuthorName = c.Author?.DisplayName ?? string.Empty,
            Topic = c.Topic,
            Difficulty = c.Difficulty,
            Language = c.Language,
            ListPrice = c.ListPrice,
            CoverImageUrl = c.CoverImageUrl,
            IsPublished = c.IsPublished,
            IsApproved = c.IsApprovedByAdmin,
            TotalRuntimeMinutes = c.TotalRuntimeMinutes,
            TotalRegistrations = c.TotalRegistrations,
            CreatedOn = c.CreatedOn,
            LastModifiedOn = c.LastModifiedOn,
            AverageRating = c.FeedbackEntries?.Where(f => f.IsApproved)
                .Select(f => (double?)f.StarRating).DefaultIfEmpty().Average() ?? 0,
            ApprovedReviewCount = c.FeedbackEntries?.Count(f => f.IsApproved) ?? 0
        };
    }
}
