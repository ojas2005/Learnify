using Learnify.Core.Domain;
using Learnify.Reviews.API.Contracts;

namespace Learnify.Reviews.API.Storage;

public interface IReviewStore
{
    Task<CourseFeedback?> GetByIdAsync(int reviewId);
    Task<List<CourseFeedback>> GetPendingReviewsAsync();
    Task<List<CourseFeedback>> GetAllReviewsAsync();
    Task<ReviewModerationStats> GetModerationStatsAsync();
    Task<CourseFeedback> UpdateAsync(CourseFeedback review);
    Task DeleteAsync(int reviewId);
}
