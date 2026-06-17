using Learnify.Core.Enums;
using Learnify.Analytics.API.Contracts;

namespace Learnify.Analytics.API.Storage;

public interface IAnalyticsRepository
{
    // User analytics
    Task<int> GetTotalUsersAsync();
    Task<int> GetActiveUsersAsync();
    Task<Dictionary<PlatformRole, int>> GetUsersByRoleAsync();
    Task<int> GetNewUsersThisMonthAsync();
    Task<int> GetSuspendedUsersAsync();

    // Course analytics
    Task<int> GetTotalCoursesAsync();
    Task<int> GetPublishedCoursesAsync();
    Task<int> GetDraftCoursesAsync();
    Task<int> GetPendingApprovalCoursesAsync();
    Task<int> GetNewCoursesThisMonthAsync();
    Task<double> GetAverageCourseRatingAsync();

    // Enrollment analytics
    Task<int> GetTotalEnrollmentsAsync();
    Task<int> GetActiveEnrollmentsAsync();
    Task<int> GetCompletedEnrollmentsAsync();
    Task<int> GetNewEnrollmentsThisMonthAsync();

    // Revenue analytics
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueThisMonthAsync();
    Task<decimal> GetRevenueLastMonthAsync();
    Task<decimal> GetAverageCoursePriceAsync();

    // Popular courses
    Task<List<PopularCourse>> GetPopularCoursesAsync(int limit = 10);

    // Time series data
    Task<List<TimeSeriesData>> GetUserGrowthDataAsync(int months = 12);
    Task<List<TimeSeriesData>> GetEnrollmentTrendsDataAsync(int months = 12);
    Task<List<TimeSeriesData>> GetRevenueTrendsDataAsync(int months = 12);

    // Individual learner stats
    Task<Dictionary<string, int>> GetLearnerStatsAsync(int learnerId);
}
