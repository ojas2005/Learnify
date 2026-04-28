using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Identity.API.Application;
using Learnify.Identity.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Learnify.Identity.API.Endpoints;

[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly IIdentityBroker _identity;

    public AccountController(IIdentityBroker identity)
    {
        _identity = identity;
    }

    //public endpoints

    [HttpPost("register")]
    [ProducesResponseType(typeof(AccountSummary),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        var result = await _identity.RegisterNewAccountAsync(
            request.DisplayName,request.Email,request.Password,request.Role);

        return result.Succeeded
            ? CreatedAtAction(nameof(GetMyProfile),ToSummary(result.Payload!))
            : ConvertFailure(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tokenResult = await _identity.AuthenticateAsync(request.Email,request.Password);
        if (!tokenResult.Succeeded)
            return Unauthorized(new { message = tokenResult.FailureReason });

        //after authenticating,fetch the profile to return alongside the token
        //note: in a real system we'd decode the token or return a minimal payload
        return Ok(new AuthTokenResponse { AccessToken = tokenResult.Payload! });
    }

    //Authenticated endpoints

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AccountSummary),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile()
    {
        var callerId = ExtractCallerId();
        var result = await _identity.FetchAccountAsync(callerId);
        return result.Succeeded ? Ok(ToSummary(result.Payload!)) : ConvertFailure(result);
    }

    [HttpGet("{accountId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(AccountSummary),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(int accountId)
    {
        var result = await _identity.FetchAccountAsync(accountId);
        return result.Succeeded ? Ok(ToSummary(result.Payload!)) : ConvertFailure(result);
    }

    [HttpPatch("me/profile")]
    [Authorize]
    [ProducesResponseType(typeof(AccountSummary),StatusCodes.Status200OK)]
    public async Task<IActionResult> AmendProfile([FromBody] ProfileAmendRequest request)
    {
        var callerId = ExtractCallerId();
        var result = await _identity.AmendProfileAsync(callerId,request.DisplayName,request.ProfilePictureUrl);
        return result.Succeeded ? Ok(ToSummary(result.Payload!)) : ConvertFailure(result);
    }

    [HttpPatch("me/password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest request)
    {
        var callerId = ExtractCallerId();
        var result = await _identity.RotatePasswordAsync(callerId,request.CurrentPassword,request.NewPassword);
        return result.Succeeded ? NoContent() : ConvertFailure(result);
    }

    //Admin-only endpoints

    [HttpGet("by-role/{role}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<AccountSummary>),StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByRole(PlatformRole role)
    {
        var accounts = await _identity.ListByRoleAsync(role);
        return Ok(accounts.Select(ToSummary));
    }

    [HttpGet("search")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<AccountSummary>),StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest(new { message = "Search term empty,enter valid term" });

        var accounts = await _identity.FindAccountsAsync(term);
        return Ok(accounts.Select(ToSummary));
    }

    [HttpPost("{accountId:int}/suspend")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendAccount(int accountId)
    {
        var result = await _identity.SuspendAccountAsync(accountId);
        return result.Succeeded ? NoContent() : ConvertFailure(result);
    }

    [HttpPost("{accountId:int}/reactivate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateAccount(int accountId)
    {
        var result = await _identity.ReactivateAccountAsync(accountId);
        return result.Succeeded ? NoContent() : ConvertFailure(result);
    }

    [HttpDelete("{accountId:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount(int accountId)
    {
        var result = await _identity.DeleteAccountAsync(accountId);
        return result.Succeeded ? NoContent() : ConvertFailure(result);
    }

    //helpers

    private int ExtractCallerId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }

    private IActionResult ConvertFailure<T>(OperationResult<T> result)
    {
        switch (result.Kind)
        {
            case FailureKind.NotFound:
                return NotFound(new { message = result.FailureReason });
            case FailureKind.Conflict:
                return Conflict(new { message = result.FailureReason });
            case FailureKind.AccessDenied:
                return Unauthorized(new { message = result.FailureReason });
            default:
                return BadRequest(new { message = result.FailureReason });
        }
    }

    private IActionResult ConvertFailure(OperationResult result)
    {
        switch (result.Kind)
        {
            case FailureKind.NotFound:
                return NotFound(new { message = result.FailureReason });
            case FailureKind.Conflict:
                return Conflict(new { message = result.FailureReason });
            case FailureKind.AccessDenied:
                return Unauthorized(new { message = result.FailureReason });
            default:
                return BadRequest(new { message = result.FailureReason });
        }
    }

    private static AccountSummary ToSummary(LearnerAccount a)
    {
        return new AccountSummary
        {
            Id = a.Id,
            DisplayName = a.DisplayName,
            Email = a.EmailAddress,
            Role = a.Role,
            ProfilePictureUrl = a.ProfilePictureUrl,
            IsActive = a.IsActive,
            RegisteredOn = a.RegisteredOn,
            LastSeenAt = a.LastSeenAt
        };
    }
}
