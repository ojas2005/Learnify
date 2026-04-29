using Learnify.Analytics.API.Contracts;
using Learnify.Analytics.API.Storage;
using Learnify.Core.Enums;
using Microsoft.Extensions.Logging;

namespace Learnify.Analytics.API.Application;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repository;
    private readonly ILogger<AnalyticsService> _log;

    public AnalyticsService(IAnalyticsRepository repository, ILogger<AnalyticsService> log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task<PlatformAnalytics> GetPlatformAnalyticsAsync()
    {
        _log.LogInformation("Generating platform analytics");
        
        var analytics = new PlatformAnalytics
        {
            Users = await GetUserAnalyticsAsync(),
            Courses = await GetCourseAnalyticsAsync(),
            Enrollments = await GetEnrollmentAnalyticsAsync(),
            Revenue = await GetRevenueAnalyticsAsync(),
            PopularCourses = await GetPopularCoursesAsync()
        };

        return analytics;
    }

    public async Task<List<TimeSeriesData>> GetUserGrowthAsync(int months = 12)
    {
        return await _repository.GetUserGrowthDataAsync(months);
    }

    public async Task<List<TimeSeriesData>> GetEnrollmentTrendsAsync(int months = 12)
    {
        return await _repository.GetEnrollmentTrendsDataAsync(months);
    }

    public async Task<List<TimeSeriesData>> GetRevenueTrendsAsync(int months = 12)
    {
        return await _repository.GetRevenueTrendsDataAsync(months);
    }

    private async Task<UserAnalytics> GetUserAnalyticsAsync()
    {
        var totalUsers = await _repository.GetTotalUsersAsync();
        var activeUsers = await _repository.GetActiveUsersAsync();
        var usersByRole = await _repository.GetUsersByRoleAsync();
        var newUsersThisMonth = await _repository.GetNewUsersThisMonthAsync();
        var suspendedUsers = await _repository.GetSuspendedUsersAsync();

        return new UserAnalytics
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            Learners = usersByRole.GetValueOrDefault(PlatformRole.Learner, 0),
            Instructors = usersByRole.GetValueOrDefault(PlatformRole.Instructor, 0),
            Administrators = usersByRole.GetValueOrDefault(PlatformRole.Administrator, 0),
            NewUsersThisMonth = newUsersThisMonth,
            SuspendedUsers = suspendedUsers
        };
    }

    private async Task<CourseAnalytics> GetCourseAnalyticsAsync()
    {
        var totalCourses = await _repository.GetTotalCoursesAsync();
        var publishedCourses = await _repository.GetPublishedCoursesAsync();
        var draftCourses = await _repository.GetDraftCoursesAsync();
        var pendingApproval = await _repository.GetPendingApprovalCoursesAsync();
        var newCoursesThisMonth = await _repository.GetNewCoursesThisMonthAsync();
        var averageRating = await _repository.GetAverageCourseRatingAsync();

        return new CourseAnalytics
        {
            TotalCourses = totalCourses,
            PublishedCourses = publishedCourses,
            DraftCourses = draftCourses,
            PendingApproval = pendingApproval,
            NewCoursesThisMonth = newCoursesThisMonth,
            AverageRating = averageRating
        };
    }

    private async Task<EnrollmentAnalytics> GetEnrollmentAnalyticsAsync()
    {
        var totalEnrollments = await _repository.GetTotalEnrollmentsAsync();
        var activeEnrollments = await _repository.GetActiveEnrollmentsAsync();
        var completedEnrollments = await _repository.GetCompletedEnrollmentsAsync();
        var newEnrollmentsThisMonth = await _repository.GetNewEnrollmentsThisMonthAsync();
        var completionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments * 100 : 0;

        return new EnrollmentAnalytics
        {
            TotalEnrollments = totalEnrollments,
            ActiveEnrollments = activeEnrollments,
            CompletedEnrollments = completedEnrollments,
            NewEnrollmentsThisMonth = newEnrollmentsThisMonth,
            CompletionRate = completionRate
        };
    }

    private async Task<RevenueAnalytics> GetRevenueAnalyticsAsync()
    {
        var totalRevenue = await _repository.GetTotalRevenueAsync();
        var revenueThisMonth = await _repository.GetRevenueThisMonthAsync();
        var revenueLastMonth = await _repository.GetRevenueLastMonthAsync();
        var averageCoursePrice = await _repository.GetAverageCoursePriceAsync();
        var totalUsers = await _repository.GetTotalUsersAsync();
        var revenuePerUser = totalUsers > 0 ? totalRevenue / totalUsers : 0;

        return new RevenueAnalytics
        {
            TotalRevenue = totalRevenue,
            RevenueThisMonth = revenueThisMonth,
            RevenueLastMonth = revenueLastMonth,
            AverageCoursePrice = averageCoursePrice,
            RevenuePerUser = revenuePerUser
        };
    }

    private async Task<List<PopularCourse>> GetPopularCoursesAsync()
    {
        return await _repository.GetPopularCoursesAsync(10);
    }

    public async Task<Dictionary<string, int>> GetLearnerStatsAsync(int learnerId)
    {
        return await _repository.GetLearnerStatsAsync(learnerId);
    }
}
