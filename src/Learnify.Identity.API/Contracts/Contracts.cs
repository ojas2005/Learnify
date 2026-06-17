using Learnify.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Identity.API.Contracts;

public class RegistrationRequest
{
    [Required,StringLength(100,MinimumLength = 2)]
    public string DisplayName{get;set;}= string.Empty;

    [Required,EmailAddress,StringLength(255)]
    public string Email{get;set;}= string.Empty;

    [Required,StringLength(100,MinimumLength = 8)]
    public string Password{get;set;}= string.Empty;

    [Required]
    public PlatformRole Role { get; set; }
}

public class LoginRequest
{
    [Required,EmailAddress]
    public string Email{get;set;}= string.Empty;

    [Required]
    public string Password{get;set;}= string.Empty;
}

public class ProfileAmendRequest
{
    [Required,StringLength(100,MinimumLength = 2)]
    public string DisplayName{get;set;}= string.Empty;

    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }
}

public class PasswordChangeRequest
{
    [Required]
    public string CurrentPassword{get;set;}= string.Empty;

    [Required,StringLength(100,MinimumLength = 8)]
    public string NewPassword{get;set;}= string.Empty;
}

public class AccountSummary
{
    public int Id { get; set; }
    public string DisplayName{get;set;}= string.Empty;
    public string Email{get;set;}= string.Empty;
    public PlatformRole Role { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredOn { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

public class AuthTokenResponse
{
    public string AccessToken{get;set;}= string.Empty;
    public string TokenType{get;set;}= "Bearer";
}
