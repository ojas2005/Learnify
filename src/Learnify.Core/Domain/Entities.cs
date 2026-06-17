using Learnify.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Core.Domain;
public class CourseRegistration
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public int CourseId { get; set; }
    public DateTime RegisteredOn{get;set;}= DateTime.UtcNow;
    public DateTime? FinishedOn { get; set; }
    public RegistrationStatus Status{get;set;}= RegistrationStatus.Active;

    public int CompletionPercent{get;set;}= 0; //percent of lessons completed,0–100

    public DateTime? LastOpenedOn { get; set; }

    public bool CredentialIssued{get;set;}= false; //Prevents duplicate certificate generation,set to true once issued.

    [StringLength(100)]
    public string? PaymentReference { get; set; }

    // Navigation
    public virtual LearnerAccount Learner{get;set;}= null!;
    public virtual CourseOffering Course{get;set;}= null!;
    public virtual ICollection<LessonWatchRecord> WatchRecords{get;set;}= new List<LessonWatchRecord>();
}

public class CurriculumLesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }

    [Required, StringLength(200)]
    public string Title{get;set;}= string.Empty;

    [StringLength(2000)]
    public string? Body { get; set; }

    public MediaFormat Format { get; set; }

    [StringLength(500)]
    public string? MediaUrl { get; set; }

    //display order
    public int SequencePosition { get; set; }

    //duration of videos
    public int DurationMinutes { get; set; }

    public bool IsPublished{get;set;}= false;

    //preview lessons are visible without enrollment
    public bool IsPreviewable{get;set;}= false;

    public DateTime AddedOn{get;set;}= DateTime.UtcNow;

    // Navigation
    public virtual CourseOffering Course{get;set;}= null!;
    public virtual ICollection<LessonWatchRecord> WatchRecords{get;set;}= new List<LessonWatchRecord>();
}


public class LessonWatchRecord
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public bool IsFinished{get;set;}= false;
    public DateTime? FinishedOn { get; set; }

    //to keep a track of how much completed,in seconds
    public int SecondsWatched{get;set;}= 0;

    public DateTime LastWatchedOn{get;set;}= DateTime.UtcNow;

    // Navigation
    public virtual LearnerAccount Learner{get;set;}= null!;
    public virtual CurriculumLesson Lesson{get;set;}= null!;
}

public class CourseExam
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int? LessonId { get; set; }

    [Required, StringLength(200)]
    public string Title{get;set;}= string.Empty;

    //json list of questions with options and correct answers
    public string QuestionsPayload{get;set;}= "[]";

    //passong marks
    public int PassThreshold{get;set;}= 70;

    //number of times a learner may attempt before being locked out
    public int AttemptsAllowed{get;set;}= 3;

    public bool IsPublished{get;set;}= false;
    public DateTime CreatedOn{get;set;}= DateTime.UtcNow;

    // Navigation
    public virtual CourseOffering Course{get;set;}= null!;
    public virtual ICollection<ExamAttempt> Attempts{get;set;}= new List<ExamAttempt>();
}


public class ExamAttempt
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public int LearnerId { get; set; }
    public DateTime BeganAt{get;set;}= DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    //score 0-100
    public int Score{get;set;}= 0;

    public bool HasPassed{get;set;}= false;

    //answers submitted by the learner
    public string AnswersPayload{get;set;}= string.Empty;

    // Navigation
    public virtual CourseExam Exam{get;set;}= null!;
    public virtual LearnerAccount Learner{get;set;}= null!;
}

public class CourseFeedback
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public int CourseId { get; set; }

    //rating 1-5
    [Range(1, 5)]
    public int StarRating { get; set; }

    [StringLength(2000)]
    public string? ReviewText { get; set; }

    //Moderation flag,admin reviews before making visible to other learners.
    public bool IsApproved{get;set;}= false;

    public DateTime SubmittedOn{get;set;}= DateTime.UtcNow;
    public DateTime? LastEditedOn { get; set; }

    // Navigation
    public virtual LearnerAccount Learner{get;set;}= null!;
    public virtual CourseOffering Course{get;set;}= null!;
}

//a completion credential awarded when a learner finishes a course with 100% progress,contains a verifiable unique code for third-party validation.
public class CompletionCredential
{
    public int Id { get; set; }

    [StringLength(32)]
    public string CredentialCode{get;set;}= string.Empty;

    public int LearnerId { get; set; }
    public int CourseId { get; set; }
    public DateTime AwardedOn{get;set;}= DateTime.UtcNow;

    [StringLength(500)]
    public string? DocumentUrl { get; set; }

    //Short unique code for external verification portals
    [StringLength(32)]
    public string VerificationPin{get;set;}= string.Empty;

    // Navigation
    public virtual LearnerAccount Learner{get;set;}= null!;
    public virtual CourseOffering Course{get;set;}= null!;
}

//Platform audit trail for sensitive operations.
public class AuditEntry
{
    public int Id { get; set; }
    public int? ActorId { get; set; }
    public string Action{get;set;}= string.Empty;
    public string EntityType{get;set;}= string.Empty;
    public string? EntityId { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public DateTime OccurredAt{get;set;}= DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
