using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Identity.API.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Learnify.Identity.API.Security; // Add this line
namespace Learnify.Identity.API.Application;

public class IdentityBroker : IIdentityBroker
{
    private readonly ILearnerStore _learnerStore;
    private readonly IPasswordHasher<LearnerAccount> _passwordHasher;
    private readonly TokenMinter _tokenMinter;
    private readonly ILogger<IdentityBroker> _log;

    public IdentityBroker(
        ILearnerStore learnerStore,
        IPasswordHasher<LearnerAccount> passwordHasher,
        TokenMinter tokenMinter,
        ILogger<IdentityBroker> log)
    {
        _learnerStore = learnerStore;
        _passwordHasher = passwordHasher;
        _tokenMinter = tokenMinter;
        _log = log;
    }

    public async Task<OperationResult<LearnerAccount>> RegisterNewAccountAsync(string displayName,string email,string rawPassword,PlatformRole role)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        // Block duplicate registrations before touching DB
        if (await _learnerStore.EmailAlreadyTakenAsync(normalizedEmail))
        {
            _log.LogWarning("Trying to register with existing email:- {Email}",normalizedEmail);
            return OperationResult<LearnerAccount>.Conflict("Email already exists");
        }

        // Basic display name sanitation
        var cleanName = displayName.Trim();
        if (cleanName.Length < 2){
            return OperationResult<LearnerAccount>.BusinessRuleViolation("name must be of 2 letters");
        }

        var account = new LearnerAccount
        {
            DisplayName = cleanName,
            EmailAddress = normalizedEmail,
            Role = role,
            IsActive = true,
            RegisteredOn = DateTime.UtcNow
        };

        account.HashedPassword = _passwordHasher.HashPassword(account,rawPassword);

        var saved = await _learnerStore.PersistNewAccountAsync(account);
        _log.LogInformation("Account registered: {AccountId} ({Role})",saved.Id,role);

        return OperationResult<LearnerAccount>.Ok(saved);
    }

    public async Task<OperationResult<string>> AuthenticateAsync(string email,string rawPassword)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var account = await _learnerStore.LookupByEmailAsync(normalizedEmail);

        //we will be giving the same message for both "not found" and "wrong password" to prevent email enumeration attacks.
        if (account is null || !account.IsActive)
        {
            _log.LogWarning("login attempt failed:- {Email}",normalizedEmail);
            return OperationResult<string>.AccessDenied("Email or password is incorrect");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            account,account.HashedPassword,rawPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _log.LogWarning("Password mismatch for account {AccountId}",account.Id);
            return OperationResult<string>.AccessDenied("Email or password is incorrect");
        }

        //if the hasher used an older algorithm,rehash silently
        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            account.HashedPassword = _passwordHasher.HashPassword(account,rawPassword);
            await _learnerStore.SaveChangesAsync();
            _log.LogInformation("Password hash upgraded for account {AccountId}",account.Id);
        }

        await _learnerStore.RecordLoginTimestampAsync(account.Id);

        var token = _tokenMinter.IssueToken(account);
        _log.LogInformation("Successful login for account {AccountId}",account.Id);

        return OperationResult<string>.Ok(token);
    }

    public async Task<OperationResult<LearnerAccount>> FetchAccountAsync(int accountId)
    {
        var account = await _learnerStore.LookupByIdAsync(accountId);
        return account is null
            ? OperationResult<LearnerAccount>.NotFound($"Account {accountId} does not exist")
            : OperationResult<LearnerAccount>.Ok(account);
    }

    public async Task<OperationResult<LearnerAccount>> AmendProfileAsync(
        int accountId,
        string newDisplayName,
        string? newProfilePicUrl)
    {
        var account = await _learnerStore.LookupByIdAsync(accountId);
        if (account is null)
            return OperationResult<LearnerAccount>.NotFound("Account not found");

        var trimmedName = newDisplayName.Trim();
        if (trimmedName.Length < 2)
            return OperationResult<LearnerAccount>.BusinessRuleViolation("Display name is too short");

        account.DisplayName = trimmedName;

        //only update the picture if a value was actually provided
        if (!string.IsNullOrWhiteSpace(newProfilePicUrl))
            account.ProfilePictureUrl = newProfilePicUrl;

        var updated = await _learnerStore.SaveUpdatedAccountAsync(account);
        return OperationResult<LearnerAccount>.Ok(updated);
    }

    public async Task<OperationResult> RotatePasswordAsync(
        int accountId,
        string currentPassword,
        string newPassword)
    {
        var account = await _learnerStore.LookupByIdAsync(accountId);
        if (account is null)
            return OperationResult.NotFound("Account not found.");

        if (newPassword.Length < 8)
            return OperationResult.BusinessRuleViolation("New password must be at least 8 characters.");

        var check = _passwordHasher.VerifyHashedPassword(account,account.HashedPassword,currentPassword);
        if (check == PasswordVerificationResult.Failed)
            return OperationResult.AccessDenied("Current password is incorrect.");

        account.HashedPassword = _passwordHasher.HashPassword(account,newPassword);
        await _learnerStore.SaveUpdatedAccountAsync(account);

        _log.LogInformation("Password rotated for account {AccountId}",accountId);
        return OperationResult.Done;
    }

    public async Task<OperationResult> SuspendAccountAsync(int accountId)
    {
        var account = await _learnerStore.LookupByIdAsync(accountId);
        if (account is null)
            return OperationResult.NotFound("Account not found.");

        if (!account.IsActive)
            return OperationResult.BusinessRuleViolation("Account is already suspended.");

        account.IsActive = false;
        await _learnerStore.SaveUpdatedAccountAsync(account);

        _log.LogWarning("Account {AccountId} suspended",accountId);
        return OperationResult.Done;
    }

    public async Task<OperationResult> ReactivateAccountAsync(int accountId)
    {
        var account = await _learnerStore.LookupByIdAsync(accountId);
        if (account is null)
            return OperationResult.NotFound("Account not found.");

        if (account.IsActive)
            return OperationResult.BusinessRuleViolation("Account is already active.");

        account.IsActive = true;
        await _learnerStore.SaveUpdatedAccountAsync(account);

        _log.LogInformation("Account {AccountId} reactivated", accountId);
        return OperationResult.Done;
    }

    public async Task<OperationResult> DeleteAccountAsync(int accountId)
    {
        var account = await _learnerStore.LookupByIdAsync(accountId);
        if (account is null)
            return OperationResult.NotFound("Account not found.");

        // Check for active enrollments or other constraints
        var hasEnrollments = await _learnerStore.HasActiveEnrollmentsAsync(accountId);
        if (hasEnrollments)
            return OperationResult.BusinessRuleViolation("Cannot delete account with active course enrollments.");

        await _learnerStore.DeleteAccountAsync(accountId);

        _log.LogWarning("Account {AccountId} permanently deleted", accountId);
        return OperationResult.Done;
    }

    public async Task<List<LearnerAccount>> ListByRoleAsync(PlatformRole role)
    {
        return await _learnerStore.FetchAllByRoleAsync(role);
    }

    public async Task<List<LearnerAccount>> FindAccountsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<LearnerAccount>();
        }

        return await _learnerStore.SearchByNameOrEmailAsync(searchTerm.Trim());
    }

    public bool ValidateToken(string token)
    {
        return _tokenMinter.ValidateToken(token);
    }
}
