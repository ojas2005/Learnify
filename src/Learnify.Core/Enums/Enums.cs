namespace Learnify.Core.Enums;

//role of the user
public enum PlatformRole
{
    Learner = 0,
    Instructor = 1,
    Administrator = 2
}

//Difficulty level of a course,shown to learners before enrolling
public enum DifficultyTier
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2,
    Expert = 3
}

//Lifecycle state of a learner's course seat
public enum RegistrationStatus
{
    Active = 0,
    Completed = 1,
    Withdrawn = 2
}

//Type of media used for a lesson.
public enum MediaFormat
{
    Article = 0,
    Video = 1,
    Audio = 2,
    Slides = 3,
    Interactive = 4
}
