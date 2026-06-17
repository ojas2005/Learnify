using Learnify.Core.Enums;

namespace Learnify.Analytics.API.Contracts;

public class PlatformAnalytics
{
    public UserAnalytics Users { get; set; } = new();
    public CourseAnalytics Courses { get; set; } = new();
    public EnrollmentAnalytics Enrollments { get; set; } = new();
    public RevenueAnalytics Revenue { get; set; } = new();
    public List<PopularCourse> PopularCourses { get; set; } = new();
}

public class UserAnalytics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int Learners { get; set; }
    public int Instructors { get; set; }
    public int Administrators { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int SuspendedUsers { get; set; }
}

public class CourseAnalytics
{
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int DraftCourses { get; set; }
    public int PendingApproval { get; set; }
    public int NewCoursesThisMonth { get; set; }
    public double AverageRating { get; set; }
}

public class EnrollmentAnalytics
{
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public int NewEnrollmentsThisMonth { get; set; }
    public double CompletionRate { get; set; }
}

public class RevenueAnalytics
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public decimal AverageCoursePrice { get; set; }
    public decimal RevenuePerUser { get; set; }
}

public class PopularCourse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
    public double AverageRating { get; set; }
    public decimal Revenue { get; set; }
}

public class TimeSeriesData
{
    public string Period { get; set; } = string.Empty;
    public int Value { get; set; }
    public DateTime Date { get; set; }
}
