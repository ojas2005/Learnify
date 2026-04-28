using Learnify.Core.Domain;
using Learnify.Core.Enums;

namespace Learnify.Identity.API.Storage;

public interface ILearnerStore
{
    Task<LearnerAccount?> LookupByEmailAsync(string normalizedEmail);
    Task<LearnerAccount?> LookupByIdAsync(int accountId);
    Task<bool> EmailAlreadyTakenAsync(string normalizedEmail);
    Task<List<LearnerAccount>> FetchAllByRoleAsync(PlatformRole role);
    Task<List<LearnerAccount>> FetchAllActiveAsync();
    Task<List<LearnerAccount>> SearchByNameOrEmailAsync(string term);
    Task RecordLoginTimestampAsync(int accountId);
    Task<LearnerAccount> PersistNewAccountAsync(LearnerAccount account);
    Task<LearnerAccount> SaveUpdatedAccountAsync(LearnerAccount account);
    Task<bool> HasActiveEnrollmentsAsync(int accountId);
    Task DeleteAccountAsync(int accountId);
    Task SaveChangesAsync();
}
