using Learnify.Analytics.API.Contracts;

namespace Learnify.Analytics.API.Application;

public interface IAnalyticsService
{
    Task<PlatformAnalytics> GetPlatformAnalyticsAsync();
    Task<List<TimeSeriesData>> GetUserGrowthAsync(int months = 12);
    Task<List<TimeSeriesData>> GetEnrollmentTrendsAsync(int months = 12);
    Task<List<TimeSeriesData>> GetRevenueTrendsAsync(int months = 12);
    Task<Dictionary<string, int>> GetLearnerStatsAsync(int learnerId);
}
