using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Reviews.API.Application;
using Learnify.Reviews.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnify.Reviews.API.Endpoints;

[ApiController]
[Route("api/admin/reviews")]
[Produces("application/json")]
[Authorize(Roles = "Administrator")]
public class ReviewModerationController : ControllerBase
{
    private readonly IReviewModerator _reviewModerator;

    public ReviewModerationController(IReviewModerator reviewModerator)
    {
        _reviewModerator = reviewModerator;
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<ReviewSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingReviews()
    {
        var reviews = await _reviewModerator.GetPendingReviewsAsync();
        var summaries = reviews.Select(ToReviewSummary);
        return Ok(summaries);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<ReviewSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllReviews()
    {
        var reviews = await _reviewModerator.GetAllReviewsAsync();
        var summaries = reviews.Select(ToReviewSummary);
        return Ok(summaries);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(ReviewModerationStats), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModerationStats()
    {
        var stats = await _reviewModerator.GetModerationStatsAsync();
        return Ok(stats);
    }

    [HttpPost("{reviewId:int}/approve")]
    [ProducesResponseType(typeof(ReviewSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApproveReview(int reviewId)
    {
        var result = await _reviewModerator.ApproveReviewAsync(reviewId);
        return result.Succeeded ? Ok(ToReviewSummary(result.Payload!)) : ConvertFailure(result);
    }

    [HttpPost("{reviewId:int}/reject")]
    [ProducesResponseType(typeof(ReviewSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectReview(int reviewId)
    {
        var result = await _reviewModerator.RejectReviewAsync(reviewId);
        return result.Succeeded ? Ok(ToReviewSummary(result.Payload!)) : ConvertFailure(result);
    }

    [HttpDelete("{reviewId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var result = await _reviewModerator.DeleteReviewAsync(reviewId);
        return result.Succeeded ? NoContent() : ConvertFailure(result);
    }

    private static ReviewSummary ToReviewSummary(CourseFeedback f)
    {
        return new ReviewSummary
        {
            Id = f.Id,
            LearnerId = f.LearnerId,
            LearnerName = f.Learner?.DisplayName ?? string.Empty,
            CourseId = f.CourseId,
            CourseTitle = f.Course?.Title ?? string.Empty,
            StarRating = f.StarRating,
            ReviewText = f.ReviewText,
            IsApproved = f.IsApproved,
            SubmittedOn = f.SubmittedOn,
            LastEditedOn = f.LastEditedOn
        };
    }

    private IActionResult ConvertFailure<T>(OperationResult<T> result)
    {
        return result.Kind switch
        {
            FailureKind.NotFound => NotFound(new { message = result.FailureReason }),
            FailureKind.Conflict => Conflict(new { message = result.FailureReason }),
            FailureKind.AccessDenied => Forbid(),
            _ => BadRequest(new { message = result.FailureReason })
        };
    }

    private IActionResult ConvertFailure(OperationResult result)
    {
        return result.Kind switch
        {
            FailureKind.NotFound => NotFound(new { message = result.FailureReason }),
            FailureKind.Conflict => Conflict(new { message = result.FailureReason }),
            FailureKind.AccessDenied => Forbid(),
            _ => BadRequest(new { message = result.FailureReason })
        };
    }
}
