using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Reviews.API.Contracts;
using Learnify.Reviews.API.Storage;
using Microsoft.Extensions.Logging;

namespace Learnify.Reviews.API.Application;

public class ReviewModerator : IReviewModerator
{
    private readonly IReviewStore _reviewStore;
    private readonly ILogger<ReviewModerator> _log;

    public ReviewModerator(IReviewStore reviewStore, ILogger<ReviewModerator> log)
    {
        _reviewStore = reviewStore;
        _log = log;
    }

    public async Task<OperationResult<CourseFeedback>> ApproveReviewAsync(int reviewId)
    {
        var review = await _reviewStore.GetByIdAsync(reviewId);
        if (review is null)
            return OperationResult<CourseFeedback>.NotFound("Review not found.");

        if (review.IsApproved)
            return OperationResult<CourseFeedback>.Conflict("Review is already approved.");

        review.IsApproved = true;
        review.LastEditedOn = DateTime.UtcNow;

        var updated = await _reviewStore.UpdateAsync(review);
        _log.LogInformation("Review {ReviewId} approved by admin", reviewId);

        return OperationResult<CourseFeedback>.Ok(updated);
    }

    public async Task<OperationResult<CourseFeedback>> RejectReviewAsync(int reviewId)
    {
        var review = await _reviewStore.GetByIdAsync(reviewId);
        if (review is null)
            return OperationResult<CourseFeedback>.NotFound("Review not found.");

        if (!review.IsApproved)
            return OperationResult<CourseFeedback>.Conflict("Review is already in rejected state.");

        review.IsApproved = false;
        review.LastEditedOn = DateTime.UtcNow;

        var updated = await _reviewStore.UpdateAsync(review);
        _log.LogInformation("Review {ReviewId} rejected by admin", reviewId);

        return OperationResult<CourseFeedback>.Ok(updated);
    }

    public async Task<OperationResult<CourseFeedback>> DeleteReviewAsync(int reviewId)
    {
        var review = await _reviewStore.GetByIdAsync(reviewId);
        if (review is null)
            return OperationResult<CourseFeedback>.NotFound("Review not found.");

        await _reviewStore.DeleteAsync(reviewId);
        _log.LogWarning("Review {ReviewId} permanently deleted by admin", reviewId);

        return OperationResult<CourseFeedback>.Ok(review);
    }

    public async Task<List<CourseFeedback>> GetPendingReviewsAsync()
    {
        return await _reviewStore.GetPendingReviewsAsync();
    }

    public async Task<List<CourseFeedback>> GetAllReviewsAsync()
    {
        return await _reviewStore.GetAllReviewsAsync();
    }

    public async Task<ReviewModerationStats> GetModerationStatsAsync()
    {
        var stats = await _reviewStore.GetModerationStatsAsync();
        return stats;
    }

    public async Task<OperationResult<CourseFeedback>> SubmitReviewAsync(int learnerId, int courseId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            return OperationResult<CourseFeedback>.BadRequest("Rating must be between 1 and 5.");

        var review = new CourseFeedback
        {
            LearnerId = learnerId,
            CourseId = courseId,
            StarRating = rating,
            ReviewText = comment,
            SubmittedOn = DateTime.UtcNow,
            IsApproved = false // require moderation by default
        };

        var saved = await _reviewStore.InsertAsync(review);
        _log.LogInformation("New review submitted for course {CourseId} by learner {LearnerId}", courseId, learnerId);

        return OperationResult<CourseFeedback>.Ok(saved);
    }
}
