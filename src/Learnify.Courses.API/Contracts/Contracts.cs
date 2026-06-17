using Learnify.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Courses.API.Contracts;

public class CreateCourseRequest
{
    [Required, StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Synopsis { get; set; }

    [Required, StringLength(100)]
    public string Topic { get; set; } = string.Empty;

    public DifficultyTier Difficulty { get; set; }

    [StringLength(50)]
    public string Language { get; set; } = "English";

    [Range(0, 99999.99)]
    public decimal ListPrice { get; set; }

    [StringLength(500)]
    public string? CoverImageUrl { get; set; }
}

public class UpdateCourseRequest
{
    [Required, StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Synopsis { get; set; }

    [Required, StringLength(100)]
    public string Topic { get; set; } = string.Empty;

    public DifficultyTier Difficulty { get; set; }

    [StringLength(50)]
    public string Language { get; set; } = "English";

    [Range(0, 99999.99)]
    public decimal ListPrice { get; set; }

    [StringLength(500)]
    public string? CoverImageUrl { get; set; }
}

public class CourseView
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Synopsis { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DifficultyTier Difficulty { get; set; }
    public string Language { get; set; } = string.Empty;
    public decimal ListPrice { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public bool IsApproved { get; set; }
    public int TotalRuntimeMinutes { get; set; }
    public int TotalRegistrations { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime LastModifiedOn { get; set; }
    public double AverageRating { get; set; }
    public int ApprovedReviewCount { get; set; }
}
