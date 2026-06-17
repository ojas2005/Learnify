using Learnify.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnify.Core.Domain;

//Represents a course offering in the platform."Course" is the primary product,instructors build it,students consume it.
public class CourseOffering
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title{get;set;}= string.Empty;

    [StringLength(1000)]
    public string? Synopsis { get; set; }

    public int AuthorId { get; set; }

    [StringLength(100)]
    public string Topic{get;set;}= string.Empty;

    public DifficultyTier Difficulty { get; set; }

    [StringLength(50)]
    public string Language{get;set;}= "English";

    [Column(TypeName = "decimal(10,2)")]
    public decimal ListPrice { get; set; }

    [StringLength(500)]
    public string? CoverImageUrl { get; set; }

    public bool IsPublished{get;set;}= false; //by default not created,instructor will publish it manually

    public bool IsApprovedByAdmin{get;set;}= false; //after published by instructor,admin approval required

    public DateTime CreatedOn{get;set;}= DateTime.UtcNow;
    public DateTime LastModifiedOn{get;set;}= DateTime.UtcNow;

    public int TotalRuntimeMinutes{get;set;}= 0; //Total runtime of all lessons in minutes

    public int TotalRegistrations{get;set;}= 0;

    // Navigation properties
    public virtual LearnerAccount Author{get;set;}= null!;
    public virtual ICollection<CurriculumLesson> Lessons{get;set;}= new List<CurriculumLesson>();
    public virtual ICollection<CourseRegistration> Registrations{get;set;}= new List<CourseRegistration>();
    public virtual ICollection<CourseExam> Exams{get;set;}= new List<CourseExam>();
    public virtual ICollection<CourseFeedback> FeedbackEntries{get;set;}= new List<CourseFeedback>();
}
