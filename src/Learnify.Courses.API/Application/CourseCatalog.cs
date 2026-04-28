using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Courses.API.Storage;
using Microsoft.Extensions.Logging;

namespace Learnify.Courses.API.Application;

//governs all lifecycle operations on course offerings—creation,discovery,publication,and removal.publishing flows through a two-phase gate:1.instructor explicitly publishes (makes visible for review) 2.admin approves (makes visible to learners) this prevents unapproved content from reaching learners.
public class CourseCatalog : ICourseCatalog
{
    private readonly ICourseStore _store;
    private readonly ILogger<CourseCatalog> _log;

    public CourseCatalog(ICourseStore store, ILogger<CourseCatalog> log)
    {
        _store = store;
        _log = log;
    }

    public async Task<OperationResult<CourseOffering>> ListNewCourseAsync(
        CourseOffering draft, int authorId)
    {
        //sanitize before persisting
        draft.Title = draft.Title.Trim();
        draft.Topic = draft.Topic.Trim();

        if (string.IsNullOrEmpty(draft.Title))
            return OperationResult<CourseOffering>.BusinessRuleViolation("Course title cannot be blank.");

        if (draft.ListPrice < 0)
            return OperationResult<CourseOffering>.BusinessRuleViolation("Price cannot be negative.");

        //stamp creation metadata—never trust what the caller sends for these
        draft.AuthorId = authorId;
        draft.CreatedOn = DateTime.UtcNow;
        draft.LastModifiedOn = DateTime.UtcNow;
        draft.IsPublished = false;
        draft.IsApprovedByAdmin = false;
        draft.TotalRegistrations = 0;

        var created = await _store.AddCourseAsync(draft);
        _log.LogInformation("Course {CourseId} created by instructor {AuthorId}", created.Id, authorId);

        return OperationResult<CourseOffering>.Ok(created);
    }

    public async Task<OperationResult<CourseOffering>> GetCourseAsync(int courseId)
    {
        var course = await _store.GetByIdAsync(courseId);
        return course is null
            ? OperationResult<CourseOffering>.NotFound($"Course {courseId} does not exist.")
            : OperationResult<CourseOffering>.Ok(course);
    }

    public async Task<List<CourseOffering>> GetByAuthorAsync(int authorId)
    {
        return await _store.GetByAuthorAsync(authorId);
    }

    public async Task<List<CourseOffering>> GetByTopicAsync(string topic)
    {
        return await _store.GetByTopicAsync(topic.Trim());
    }

    public async Task<List<CourseOffering>> BrowseLiveCatalogAsync()
    {
        return await _store.GetPublishedAndApprovedAsync();
    }

    public async Task<List<CourseOffering>> SearchAsync(string terms)
    {
        if (string.IsNullOrWhiteSpace(terms))
            return await BrowseLiveCatalogAsync();

        return await _store.FullTextSearchAsync(terms.Trim());
    }

    public async Task<List<CourseOffering>> GetHighlyRatedAsync(int limit)
    {
        if (limit <= 0 || limit > 100)
            limit = 10; //guard against silly values

        return await _store.GetTopRatedAsync(limit);
    }

    public async Task<OperationResult<CourseOffering>> ReviseDetailsAsync(
        int courseId, int requestingAuthorId, CourseOffering updates)
    {
        var existing = await _store.GetByIdAsync(courseId);
        if (existing is null)
            return OperationResult<CourseOffering>.NotFound("Course not found.");

        if (existing.AuthorId != requestingAuthorId)
            return OperationResult<CourseOffering>.AccessDenied("You can only edit your own courses.");

        //apply only the fields instructors are allowed to change
        existing.Title = updates.Title.Trim();
        existing.Synopsis = updates.Synopsis?.Trim();
        existing.Topic = updates.Topic.Trim();
        existing.Difficulty = updates.Difficulty;
        existing.Language = updates.Language;
        existing.ListPrice = updates.ListPrice;
        existing.CoverImageUrl = updates.CoverImageUrl;
        existing.LastModifiedOn = DateTime.UtcNow;

        var saved = await _store.UpdateCourseAsync(existing);
        return OperationResult<CourseOffering>.Ok(saved);
    }

    public async Task<OperationResult<CourseOffering>> SubmitForReviewAsync(
        int courseId, int requestingAuthorId)
    {
        var course = await _store.GetByIdAsync(courseId);
        if (course is null)
            return OperationResult<CourseOffering>.NotFound("Course not found.");

        if (course.AuthorId != requestingAuthorId)
            return OperationResult<CourseOffering>.AccessDenied("You can only publish your own courses.");

        if (course.IsPublished)
            return OperationResult<CourseOffering>.Conflict("Course is already submitted for review.");

        //validate the course has minimum content before allowing submission
        var readinessCheck = AssessPublicationReadiness(course);
        if (!readinessCheck.IsReady)
            return OperationResult<CourseOffering>.BusinessRuleViolation(readinessCheck.BlockingReason!);

        course.IsPublished = true;
        course.LastModifiedOn = DateTime.UtcNow;

        var updated = await _store.UpdateCourseAsync(course);
        _log.LogInformation("Course {CourseId} submitted for admin review", courseId);

        return OperationResult<CourseOffering>.Ok(updated);
    }

    public async Task<OperationResult<CourseOffering>> ApproveForLiveAsync(int courseId)
    {
        var course = await _store.GetByIdAsync(courseId);
        if (course is null)
            return OperationResult<CourseOffering>.NotFound("Course not found.");

        if (!course.IsPublished)
            return OperationResult<CourseOffering>.BusinessRuleViolation(
                "Course must be submitted for review before it can be approved.");

        if (course.IsApprovedByAdmin)
            return OperationResult<CourseOffering>.Conflict("Course is already live.");

        course.IsApprovedByAdmin = true;
        course.LastModifiedOn = DateTime.UtcNow;

        var updated = await _store.UpdateCourseAsync(course);
        _log.LogInformation("Course {CourseId} approved and now live", courseId);

        return OperationResult<CourseOffering>.Ok(updated);
    }

    public async Task<OperationResult<CourseOffering>> RejectCourseAsync(int courseId)
    {
        var course = await _store.GetByIdAsync(courseId);
        if (course is null)
            return OperationResult<CourseOffering>.NotFound("Course not found.");

        if (!course.IsPublished)
            return OperationResult<CourseOffering>.BusinessRuleViolation(
                "Course must be submitted for review before it can be rejected.");

        if (!course.IsApprovedByAdmin)
            return OperationResult<CourseOffering>.Conflict("Course is already in rejected state.");

        course.IsApprovedByAdmin = false;
        course.IsPublished = false; // Reset to draft state
        course.LastModifiedOn = DateTime.UtcNow;

        var updated = await _store.UpdateCourseAsync(course);
        _log.LogInformation("Course {CourseId} rejected and returned to draft state", courseId);

        return OperationResult<CourseOffering>.Ok(updated);
    }

    public async Task<OperationResult> RemoveCourseAsync(int courseId, int requestingUserId, bool isAdmin)
    {
        var course = await _store.GetByIdAsync(courseId);
        if (course is null)
            return OperationResult.NotFound("Course not found.");

        if (!isAdmin && course.AuthorId != requestingUserId)
            return OperationResult.AccessDenied("You can only delete your own courses.");

        //prevent deletion of courses with active enrollments unless admin overrides
        if (!isAdmin && course.TotalRegistrations > 0)
            return OperationResult.BusinessRuleViolation(
                "Cannot delete a course that has enrolled learners. Contact admin.");

        await _store.RemoveCourseAsync(courseId);
        _log.LogWarning("Course {CourseId} deleted by user {UserId} (admin={IsAdmin})",
            courseId, requestingUserId, isAdmin);

        return OperationResult.Done;
    }

    public async Task BumpRegistrationCountAsync(int courseId)
    {
        await _store.IncrementRegistrationCountAsync(courseId);
    }

    //checks minimum requirements before a course can be submitted for review.centralizing this logic here prevents the controller from needing to know about course content rules.
    private static (bool IsReady, string? BlockingReason) AssessPublicationReadiness(CourseOffering course)
    {
        if (string.IsNullOrWhiteSpace(course.Synopsis))
            return (false, "A course synopsis is required before submission.");

        if (string.IsNullOrWhiteSpace(course.CoverImageUrl))
            return (false, "A cover image is required before submission.");

        return (true, null);
    }
}
