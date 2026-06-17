using Learnify.Core.Domain;
using Learnify.Reviews.API.Contracts;
using Learnify.Reviews.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Reviews.API.Storage;

public class ReviewStore : IReviewStore
{
    private readonly ReviewsDbContext _db;

    public ReviewStore(ReviewsDbContext db)
    {
        _db = db;
    }

    public async Task<CourseFeedback?> GetByIdAsync(int reviewId)
    {
        return await _db.CourseFeedback
            .Include(f => f.Learner)
            .Include(f => f.Course)
            .FirstOrDefaultAsync(f => f.Id == reviewId);
    }

    public async Task<List<CourseFeedback>> GetPendingReviewsAsync()
    {
        return await _db.CourseFeedback
            .Include(f => f.Learner)
            .Include(f => f.Course)
            .Where(f => !f.IsApproved)
            .OrderByDescending(f => f.SubmittedOn)
            .ToListAsync();
    }

    public async Task<List<CourseFeedback>> GetAllReviewsAsync()
    {
        return await _db.CourseFeedback
            .Include(f => f.Learner)
            .Include(f => f.Course)
            .OrderByDescending(f => f.SubmittedOn)
            .ToListAsync();
    }

    public async Task<ReviewModerationStats> GetModerationStatsAsync()
    {
        var total = await _db.CourseFeedback.CountAsync();
        var pending = await _db.CourseFeedback.CountAsync(f => !f.IsApproved);
        var approved = await _db.CourseFeedback.CountAsync(f => f.IsApproved);
        var rejected = await _db.CourseFeedback.CountAsync(f => !f.IsApproved && f.LastEditedOn.HasValue);
        var avgRating = await _db.CourseFeedback.Where(f => f.IsApproved).AverageAsync(f => (double?)f.StarRating) ?? 0;

        return new ReviewModerationStats
        {
            TotalReviews = total,
            PendingReviews = pending,
            ApprovedReviews = approved,
            RejectedReviews = rejected,
            AverageRating = avgRating
        };
    }

    public async Task<CourseFeedback> UpdateAsync(CourseFeedback review)
    {
        _db.CourseFeedback.Update(review);
        await _db.SaveChangesAsync();
        return review;
    }

    public async Task<CourseFeedback> InsertAsync(CourseFeedback review)
    {
        _db.CourseFeedback.Add(review);
        await _db.SaveChangesAsync();
        return review;
    }

    public async Task DeleteAsync(int reviewId)
    {
        var review = await _db.CourseFeedback.FindAsync(reviewId);
        if (review != null)
        {
            _db.CourseFeedback.Remove(review);
            await _db.SaveChangesAsync();
        }
    }
}
