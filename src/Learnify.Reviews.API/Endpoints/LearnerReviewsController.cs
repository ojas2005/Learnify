using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Reviews.API.Application;
using Learnify.Reviews.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Learnify.Reviews.API.Endpoints;

[ApiController]
[Route("api/reviews")]
[Produces("application/json")]
public class LearnerReviewsController : ControllerBase
{
    private readonly IReviewModerator _reviewService;

    public LearnerReviewsController(IReviewModerator reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CourseFeedback), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostReview([FromBody] ReviewSubmission submission)
    {
        var callerId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        
        var result = await _reviewService.SubmitReviewAsync(
            callerId, 
            submission.CourseId, 
            submission.StarRating, 
            submission.Comment);

        return result.Succeeded ? Ok(result.Payload) : BadRequest(new { message = result.FailureReason });
    }
}

public class ReviewSubmission
{
    public int CourseId { get; set; }
    public int StarRating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
