using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Reviews.API.Contracts;

namespace Learnify.Reviews.API.Application;

public interface IReviewModerator
{
    Task<OperationResult<CourseFeedback>> ApproveReviewAsync(int reviewId);
    Task<OperationResult<CourseFeedback>> RejectReviewAsync(int reviewId);
    Task<OperationResult<CourseFeedback>> DeleteReviewAsync(int reviewId);
    Task<List<CourseFeedback>> GetPendingReviewsAsync();
    Task<List<CourseFeedback>> GetAllReviewsAsync();
    Task<ReviewModerationStats> GetModerationStatsAsync();
}
