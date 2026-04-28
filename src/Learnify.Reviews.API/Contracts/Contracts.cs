using Learnify.Core.Domain;

namespace Learnify.Reviews.API.Contracts;

public class ReviewSummary
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public string LearnerName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public string? ReviewText { get; set; }
    public bool IsApproved { get; set; }
    public DateTime SubmittedOn { get; set; }
    public DateTime? LastEditedOn { get; set; }
}

public class ApproveReviewRequest
{
    public bool IsApproved { get; set; }
}

public class ReviewModerationStats
{
    public int TotalReviews { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public int RejectedReviews { get; set; }
    public double AverageRating { get; set; }
}
