using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Registration.API.Storage;
using Microsoft.Extensions.Logging;

namespace Learnify.Registration.API.Application;

public class SeatReservation : ISeatReservation
{
    private readonly ISeatStore _seatStore;
    private readonly ILogger<SeatReservation> _log;

    public SeatReservation(ISeatStore seatStore, ILogger<SeatReservation> log)
    {
        _seatStore = seatStore;
        _log = log;
    }

    public async Task<OperationResult<CourseRegistration>> ClaimSeatAsync(int learnerId, int courseId)
    {
        // make sure student isnt already signed up
        if (await _seatStore.AlreadyRegisteredAsync(learnerId, courseId))
        {
            return OperationResult<CourseRegistration>.Conflict("You are already enrolled in this course.");
        }

        var seat = new CourseRegistration
        {
            LearnerId = learnerId,
            CourseId = courseId,
            RegisteredOn = DateTime.UtcNow,
            Status = RegistrationStatus.Active,
            CompletionPercent = 0,
            LastOpenedOn = DateTime.UtcNow,
            CredentialIssued = false
        };

        var saved = await _seatStore.CreateRegistrationAsync(seat);
        _log.LogInformation("Learner {LearnerId} registered for course {CourseId}", learnerId, courseId);

        return OperationResult<CourseRegistration>.Ok(saved);
    }

    public async Task<OperationResult<CourseRegistration>> GetRegistrationAsync(int registrationId)
    {
        // find a specific registration by its id
        var reg = await _seatStore.GetByIdAsync(registrationId);
        if (reg is null)
        {
            return OperationResult<CourseRegistration>.NotFound($"Registration {registrationId} not found.");
        }
        return OperationResult<CourseRegistration>.Ok(reg);
    }

    public async Task<List<CourseRegistration>> GetLearnerRegistrationsAsync(int learnerId)
    {
        // get all courses for a student
        return await _seatStore.GetByLearnerAsync(learnerId);
    }

    public async Task<List<CourseRegistration>> GetCourseRosterAsync(int courseId)
    {
        // get the list of students in a course
        return await _seatStore.GetByCourseAsync(courseId);
    }

    public async Task<bool> IsRegisteredAsync(int learnerId, int courseId)
    {
        // check if a student is already registered
        return await _seatStore.AlreadyRegisteredAsync(learnerId, courseId);
    }

    public async Task<OperationResult> RecordProgressAsync(int registrationId, int completionPercent)
    {
        // update how much of the course is done
        var reg = await _seatStore.GetByIdAsync(registrationId);
        if (reg is null)
        {
            return OperationResult.NotFound("Registration not found.");
        }

        if (reg.Status == RegistrationStatus.Withdrawn)
        {
            return OperationResult.BusinessRuleViolation("Cannot update progress on a withdrawn registration.");
        }

        // keep progress between 0 and 100
        var clamped = Math.Clamp(completionPercent, 0, 100);
        reg.CompletionPercent = clamped;
        reg.LastOpenedOn = DateTime.UtcNow;

        // mark as finished if progress hits 100
        if (clamped >= 100 && reg.Status == RegistrationStatus.Active)
        {
            ApplyCompletionStamp(reg);
        }

        await _seatStore.UpdateRegistrationAsync(reg);
        return OperationResult.Done;
    }

    public async Task<OperationResult> MarkFinishedAsync(int registrationId)
    {
        // mark a course as done manually
        var reg = await _seatStore.GetByIdAsync(registrationId);
        if (reg is null)
        {
            return OperationResult.NotFound("Registration not found.");
        }

        if (reg.Status == RegistrationStatus.Completed)
        {
            return OperationResult.Conflict("Course is already marked as completed.");
        }

        if (reg.Status == RegistrationStatus.Withdrawn)
        {
            return OperationResult.BusinessRuleViolation("Cannot complete a withdrawn registration.");
        }

        ApplyCompletionStamp(reg);

        await _seatStore.UpdateRegistrationAsync(reg);
        _log.LogInformation("Learner {LearnerId} completed course {CourseId}", reg.LearnerId, reg.CourseId);

        // give certificate if needed
        if (!reg.CredentialIssued)
        {
            await FlagCredentialIssuedAsync(reg);
        }

        return OperationResult.Done;
    }

    public async Task<OperationResult> WithdrawAsync(int registrationId)
    {
        // student leaves the course
        var reg = await _seatStore.GetByIdAsync(registrationId);
        if (reg is null)
        {
            return OperationResult.NotFound("Registration not found.");
        }

        if (reg.Status == RegistrationStatus.Withdrawn)
        {
            return OperationResult.Conflict("Already withdrawn from this course.");
        }

        if (reg.Status == RegistrationStatus.Completed)
        {
            return OperationResult.BusinessRuleViolation("Cannot withdraw from a completed course.");
        }

        reg.Status = RegistrationStatus.Withdrawn;
        reg.LastOpenedOn = DateTime.UtcNow;

        await _seatStore.UpdateRegistrationAsync(reg);
        _log.LogInformation("Learner {LearnerId} withdrew from course {CourseId}", reg.LearnerId, reg.CourseId);

        return OperationResult.Done;
    }

    public async Task<OperationResult> GrantCredentialAsync(int registrationId)
    {
        // official certificate approval
        var reg = await _seatStore.GetByIdAsync(registrationId);
        if (reg is null)
        {
            return OperationResult.NotFound("Registration not found.");
        }

        if (reg.Status != RegistrationStatus.Completed)
        {
            return OperationResult.BusinessRuleViolation("Credentials can only be granted for completed courses.");
        }

        if (reg.CredentialIssued)
        {
            return OperationResult.Conflict("Credential already issued for this registration.");
        }

        await FlagCredentialIssuedAsync(reg);
        return OperationResult.Done;
    }

    public async Task<List<CourseRegistration>> GetCompletedAsync(int learnerId)
    {
        // list all finished courses for a student
        return await _seatStore.GetByStatusAsync(learnerId, RegistrationStatus.Completed);
    }

    public async Task<List<CourseRegistration>> GetActiveAsync(int learnerId)
    {
        // list all ongoing courses for a student
        return await _seatStore.GetByStatusAsync(learnerId, RegistrationStatus.Active);
    }

    public async Task<int> HeadcountForCourseAsync(int courseId)
    {
        // total number of people in a course
        return await _seatStore.CountByCourseAsync(courseId);
    }

    private static void ApplyCompletionStamp(CourseRegistration reg)
    {
        // set status to finished with date and percent
        reg.Status = RegistrationStatus.Completed;
        reg.FinishedOn = DateTime.UtcNow;
        reg.CompletionPercent = 100;
        reg.LastOpenedOn = DateTime.UtcNow;
    }

    private async Task FlagCredentialIssuedAsync(CourseRegistration reg)
    {
        // mark that certificate was given
        reg.CredentialIssued = true;
        await _seatStore.UpdateRegistrationAsync(reg);
        _log.LogInformation("Credential flagged for learner {LearnerId}, course {CourseId}", reg.LearnerId, reg.CourseId);
    }
}
