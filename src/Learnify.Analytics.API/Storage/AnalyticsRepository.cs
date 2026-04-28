using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Analytics.API.Contracts;
using Learnify.Analytics.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Analytics.API.Storage;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AnalyticsDbContext _db;

    public AnalyticsRepository(AnalyticsDbContext db)
    {
        _db = db;
    }

    // User analytics
    public async Task<int> GetTotalUsersAsync()
    {
        return await _db.LearnerAccounts.CountAsync();
    }

    public async Task<int> GetActiveUsersAsync()
    {
        return await _db.LearnerAccounts.CountAsync(u => u.IsActive);
    }

    public async Task<Dictionary<PlatformRole, int>> GetUsersByRoleAsync()
    {
        return await _db.LearnerAccounts
            .GroupBy(u => u.Role)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<int> GetNewUsersThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        return await _db.LearnerAccounts.CountAsync(u => u.RegisteredOn >= firstDayOfMonth);
    }

    public async Task<int> GetSuspendedUsersAsync()
    {
        return await _db.LearnerAccounts.CountAsync(u => !u.IsActive);
    }

    // Course analytics
    public async Task<int> GetTotalCoursesAsync()
    {
        return await _db.CourseOfferings.CountAsync();
    }

    public async Task<int> GetPublishedCoursesAsync()
    {
        return await _db.CourseOfferings.CountAsync(c => c.IsPublished && c.IsApprovedByAdmin);
    }

    public async Task<int> GetDraftCoursesAsync()
    {
        return await _db.CourseOfferings.CountAsync(c => !c.IsPublished);
    }

    public async Task<int> GetPendingApprovalCoursesAsync()
    {
        return await _db.CourseOfferings.CountAsync(c => c.IsPublished && !c.IsApprovedByAdmin);
    }

    public async Task<int> GetNewCoursesThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        return await _db.CourseOfferings.CountAsync(c => c.CreatedOn >= firstDayOfMonth);
    }

    public async Task<double> GetAverageCourseRatingAsync()
    {
        return await _db.CourseFeedback
            .Where(f => f.IsApproved)
            .AverageAsync(f => (double?)f.StarRating) ?? 0;
    }

    // Enrollment analytics
    public async Task<int> GetTotalEnrollmentsAsync()
    {
        return await _db.CourseRegistrations.CountAsync();
    }

    public async Task<int> GetActiveEnrollmentsAsync()
    {
        return await _db.CourseRegistrations.CountAsync(r => r.Status == RegistrationStatus.Active);
    }

    public async Task<int> GetCompletedEnrollmentsAsync()
    {
        return await _db.CourseRegistrations.CountAsync(r => r.Status == RegistrationStatus.Completed);
    }

    public async Task<int> GetNewEnrollmentsThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        return await _db.CourseRegistrations.CountAsync(r => r.RegisteredOn >= firstDayOfMonth);
    }

    // Revenue analytics
    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _db.CourseRegistrations
            .Where(r => !string.IsNullOrEmpty(r.PaymentReference))
            .SumAsync(r => (decimal?)r.Course.ListPrice) ?? 0;
    }

    public async Task<decimal> GetRevenueThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        return await _db.CourseRegistrations
            .Where(r => r.RegisteredOn >= firstDayOfMonth && !string.IsNullOrEmpty(r.PaymentReference))
            .SumAsync(r => (decimal?)r.Course.ListPrice) ?? 0;
    }

    public async Task<decimal> GetRevenueLastMonthAsync()
    {
        var now = DateTime.UtcNow;
        var lastMonthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var lastMonthEnd = new DateTime(now.Year, now.Month, 1).AddDays(-1);
        return await _db.CourseRegistrations
            .Where(r => r.RegisteredOn >= lastMonthStart && r.RegisteredOn <= lastMonthEnd && !string.IsNullOrEmpty(r.PaymentReference))
            .SumAsync(r => (decimal?)r.Course.ListPrice) ?? 0;
    }

    public async Task<decimal> GetAverageCoursePriceAsync()
    {
        return await _db.CourseOfferings
            .Where(c => c.IsPublished && c.IsApprovedByAdmin)
            .AverageAsync(c => (decimal?)c.ListPrice) ?? 0;
    }

    // Popular courses
    public async Task<List<PopularCourse>> GetPopularCoursesAsync(int limit = 10)
    {
        return await _db.CourseOfferings
            .Where(c => c.IsPublished && c.IsApprovedByAdmin)
            .Select(c => new PopularCourse
            {
                Id = c.Id,
                Title = c.Title,
                AuthorName = c.Author.DisplayName,
                EnrollmentCount = c.TotalRegistrations,
                AverageRating = c.FeedbackEntries.Where(f => f.IsApproved).Average(f => (double?)f.StarRating) ?? 0,
                Revenue = c.CourseRegistrations.Count(r => !string.IsNullOrEmpty(r.PaymentReference)) * c.ListPrice
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(limit)
            .ToListAsync();
    }

    // Time series data
    public async Task<List<TimeSeriesData>> GetUserGrowthDataAsync(int months = 12)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddMonths(-months);
        
        return await _db.LearnerAccounts
            .Where(u => u.RegisteredOn >= startDate)
            .GroupBy(u => new { u.RegisteredOn.Year, u.RegisteredOn.Month })
            .Select(g => new TimeSeriesData
            {
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                Value = g.Count(),
                Date = new DateTime(g.Key.Year, g.Key.Month, 1)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<List<TimeSeriesData>> GetEnrollmentTrendsDataAsync(int months = 12)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddMonths(-months);
        
        return await _db.CourseRegistrations
            .Where(r => r.RegisteredOn >= startDate)
            .GroupBy(r => new { r.RegisteredOn.Year, r.RegisteredOn.Month })
            .Select(g => new TimeSeriesData
            {
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                Value = g.Count(),
                Date = new DateTime(g.Key.Year, g.Key.Month, 1)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<List<TimeSeriesData>> GetRevenueTrendsDataAsync(int months = 12)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddMonths(-months);
        
        return await _db.CourseRegistrations
            .Where(r => r.RegisteredOn >= startDate && !string.IsNullOrEmpty(r.PaymentReference))
            .GroupBy(r => new { r.RegisteredOn.Year, r.RegisteredOn.Month })
            .Select(g => new TimeSeriesData
            {
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                Value = (int)g.Sum(r => r.Course.ListPrice),
                Date = new DateTime(g.Key.Year, g.Key.Month, 1)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }
}
