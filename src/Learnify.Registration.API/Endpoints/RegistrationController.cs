using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Registration.API.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Learnify.Registration.API.Endpoints;

[ApiController]
[Route("api/registrations")]
[Produces("application/json")]
public class RegistrationController : ControllerBase
{
    private readonly ISeatReservation _seats;

    public RegistrationController(ISeatReservation seats)
    {
        _seats = seats;
    }

    [HttpPost]
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest req)
    {
        // sign up a student for a course
        var learnerId = CallerId();
        var result = await _seats.ClaimSeatAsync(learnerId, req.CourseId);
        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetRegistration), new { registrationId = result.Payload!.Id }, ToView(result.Payload));
        }
        return Fail(result);
    }

    [HttpGet("{registrationId:int}")]
    [Authorize]
    public async Task<IActionResult> GetRegistration(int registrationId)
    {
        // get info about a single registration
        var result = await _seats.GetRegistrationAsync(registrationId);
        if (!result.Succeeded)
        {
            return Fail(result);
        }

        var reg = result.Payload!;
        if (IsLearner() && reg.LearnerId != CallerId())
        {
            return Forbid();
        }

        return Ok(ToView(reg));
    }

    [HttpGet("learner/{learnerId:int}")]
    [Authorize]
    public async Task<IActionResult> GetByLearner(int learnerId)
    {
        // get all courses for a specific user
        if (IsLearner() && learnerId != CallerId())
        {
            return Forbid();
        }

        var regs = await _seats.GetLearnerRegistrationsAsync(learnerId);
        return Ok(regs.Select(ToView));
    }

    [HttpGet("learner/{learnerId:int}/active")]
    [Authorize]
    public async Task<IActionResult> GetActiveCourses(int learnerId)
    {
        // get courses that user is currently taking
        if (IsLearner() && learnerId != CallerId())
        {
            return Forbid();
        }

        var regs = await _seats.GetActiveAsync(learnerId);
        return Ok(regs.Select(ToView));
    }

    [HttpGet("learner/{learnerId:int}/completed")]
    [Authorize]
    public async Task<IActionResult> GetCompletedCourses(int learnerId)
    {
        // get courses that user has finished
        if (IsLearner() && learnerId != CallerId())
        {
            return Forbid();
        }

        var regs = await _seats.GetCompletedAsync(learnerId);
        return Ok(regs.Select(ToView));
    }

    [HttpGet("course/{courseId:int}/roster")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> GetRoster(int courseId)
    {
        // list of students in a course for teachers
        var regs = await _seats.GetCourseRosterAsync(courseId);
        return Ok(regs.Select(ToView));
    }

    [HttpGet("course/{courseId:int}/headcount")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> GetHeadcount(int courseId)
    {
        // count how many students are in a course
        var count = await _seats.HeadcountForCourseAsync(courseId);
        return Ok(new { courseId, count });
    }

    [HttpGet("check")]
    [Authorize]
    public async Task<IActionResult> CheckEnrollment([FromQuery] int learnerId, [FromQuery] int courseId)
    {
        // check if user is in a course
        if (IsLearner() && learnerId != CallerId())
        {
            return Forbid();
        }

        var enrolled = await _seats.IsRegisteredAsync(learnerId, courseId);
        return Ok(new { enrolled });
    }

    [HttpPatch("{registrationId:int}/progress")]
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> UpdateProgress(int registrationId, [FromBody] ProgressUpdate update)
    {
        // update progress for a course
        var reg = await _seats.GetRegistrationAsync(registrationId);
        if (!reg.Succeeded)
        {
            return Fail(reg);
        }
        if (reg.Payload!.LearnerId != CallerId())
        {
            return Forbid();
        }

        var result = await _seats.RecordProgressAsync(registrationId, update.CompletionPercent);
        if (result.Succeeded)
        {
            return NoContent();
        }
        return Fail(result);
    }

    [HttpPost("{registrationId:int}/complete")]
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> MarkComplete(int registrationId)
    {
        // manually finish a course
        var reg = await _seats.GetRegistrationAsync(registrationId);
        if (!reg.Succeeded)
        {
            return Fail(reg);
        }
        if (reg.Payload!.LearnerId != CallerId())
        {
            return Forbid();
        }

        var result = await _seats.MarkFinishedAsync(registrationId);
        if (result.Succeeded)
        {
            return NoContent();
        }
        return Fail(result);
    }

    [HttpPost("{registrationId:int}/withdraw")]
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> Withdraw(int registrationId)
    {
        // drop out of a course
        var reg = await _seats.GetRegistrationAsync(registrationId);
        if (!reg.Succeeded)
        {
            return Fail(reg);
        }
        if (reg.Payload!.LearnerId != CallerId())
        {
            return Forbid();
        }

        var result = await _seats.WithdrawAsync(registrationId);
        if (result.Succeeded)
        {
            return NoContent();
        }
        return Fail(result);
    }

    [HttpPost("{registrationId:int}/grant-credential")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GrantCredential(int registrationId)
    {
        // give course certificate to user
        var result = await _seats.GrantCredentialAsync(registrationId);
        if (result.Succeeded)
        {
            return NoContent();
        }
        return Fail(result);
    }

    private int CallerId()
    {
        // get the user id from login info
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            return id;
        }
        return 0;
    }

    private bool IsLearner()
    {
        // check if user is a student
        return User.IsInRole("Learner");
    }

    private IActionResult Fail<T>(OperationResult<T> r)
    {
        // handle errors when getting data
        return r.Kind switch
        {
            FailureKind.NotFound => NotFound(new { message = r.FailureReason }),
            FailureKind.Conflict => Conflict(new { message = r.FailureReason }),
            FailureKind.AccessDenied => Forbid(),
            _ => BadRequest(new { message = r.FailureReason })
        };
    }

    private IActionResult Fail(OperationResult r)
    {
        // handle simple errors
        return r.Kind switch
        {
            FailureKind.NotFound => NotFound(new { message = r.FailureReason }),
            FailureKind.Conflict => Conflict(new { message = r.FailureReason }),
            _ => BadRequest(new { message = r.FailureReason })
        };
    }

    private static RegistrationView ToView(CourseRegistration r)
    {
        // convert data for web display
        return new RegistrationView
        {
            Id = r.Id,
            LearnerId = r.LearnerId,
            LearnerName = r.Learner?.DisplayName ?? string.Empty,
            CourseId = r.CourseId,
            CourseTitle = r.Course?.Title ?? string.Empty,
            RegisteredOn = r.RegisteredOn,
            FinishedOn = r.FinishedOn,
            Status = r.Status,
            CompletionPercent = r.CompletionPercent,
            LastOpenedOn = r.LastOpenedOn,
            CredentialIssued = r.CredentialIssued,
            PaymentReference = r.PaymentReference
        };
    }
}

public class EnrollRequest
{
    [Required]
    public int CourseId { get; set; }
}

public class ProgressUpdate
{
    [Range(0, 100)]
    public int CompletionPercent { get; set; }
}

public class RegistrationView
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public string LearnerName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime RegisteredOn { get; set; }
    public DateTime? FinishedOn { get; set; }
    public RegistrationStatus Status { get; set; }
    public int CompletionPercent { get; set; }
    public DateTime? LastOpenedOn { get; set; }
    public bool CredentialIssued { get; set; }
    public string? PaymentReference { get; set; }
}
