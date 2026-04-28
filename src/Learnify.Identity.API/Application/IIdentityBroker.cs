using Learnify.Core.Core;
using Learnify.Core.Domain;
using Learnify.Core.Enums;

namespace Learnify.Identity.API.Application;

public interface IIdentityBroker
{
    Task<OperationResult<LearnerAccount>> RegisterNewAccountAsync(string displayName,string email,string rawPassword,PlatformRole role);
    Task<OperationResult<string>> AuthenticateAsync(string email,string rawPassword);
    Task<OperationResult<LearnerAccount>> FetchAccountAsync(int accountId);
    Task<OperationResult<LearnerAccount>> AmendProfileAsync(int accountId,string newDisplayName,string? newProfilePicUrl);
    Task<OperationResult> RotatePasswordAsync(int accountId,string currentPassword,string newPassword);
    Task<OperationResult> SuspendAccountAsync(int accountId);
    Task<OperationResult> ReactivateAccountAsync(int accountId);
    Task<OperationResult> DeleteAccountAsync(int accountId);
    Task<List<LearnerAccount>> ListByRoleAsync(PlatformRole role);
    Task<List<LearnerAccount>> FindAccountsAsync(string searchTerm);
    bool ValidateToken(string token);
}
