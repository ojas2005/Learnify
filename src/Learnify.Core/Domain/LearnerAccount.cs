using Learnify.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Core.Domain;
//Represents any platform participant,could be a learner,instructor,or admin.We use "LearnerAccount" because learning is the core activity even for instructors
public class LearnerAccount
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string DisplayName{get;set;}= string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string EmailAddress{get;set;}= string.Empty;

    [Required, StringLength(255)]
    public string HashedPassword{get;set;}= string.Empty;

    [Required]
    public PlatformRole Role { get; set; }

    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }

    //we never actually remove accounts,just set to false.
    public bool IsActive{get;set;}= true;

    public DateTime RegisteredOn{get;set;}= DateTime.UtcNow;

    //null until first login.useful for identifying dormant accounts.
    public DateTime? LastSeenAt { get; set; }

    // Navigation properties
    public virtual ICollection<CourseOffering> AuthoredCourses{get;set;}= new List<CourseOffering>();
    public virtual ICollection<CourseRegistration> CourseRegistrations{get;set;}= new List<CourseRegistration>();
    public virtual ICollection<LessonWatchRecord> WatchHistory{get;set;}= new List<LessonWatchRecord>();
    public virtual ICollection<ExamAttempt> ExamAttempts{get;set;}= new List<ExamAttempt>();
    public virtual ICollection<CourseFeedback> FeedbackSubmissions{get;set;}= new List<CourseFeedback>();
    public virtual ICollection<CompletionCredential> EarnedCredentials{get;set;}= new List<CompletionCredential>();
}
