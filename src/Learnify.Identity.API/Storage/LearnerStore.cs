using Learnify.Core.Domain;
using Learnify.Core.Enums;
using Learnify.Identity.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Identity.API.Storage;

//data access layer for learner accounts.
//method names describe what data is being looked up,not how it's stored.
public class LearnerStore : ILearnerStore
{
    private readonly IdentityDbContext _db;

    public LearnerStore(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<LearnerAccount?> LookupByEmailAsync(string normalizedEmail)
    {
        return await _db.Accounts.FirstOrDefaultAsync(a => a.EmailAddress == normalizedEmail);
    }

    public async Task<LearnerAccount?> LookupByIdAsync(int accountId)
    {
        return await _db.Accounts.FindAsync(accountId);
    }

    public async Task<bool> EmailAlreadyTakenAsync(string normalizedEmail)
    {
        return await _db.Accounts.AnyAsync(a => a.EmailAddress == normalizedEmail);
    }

    public async Task<List<LearnerAccount>> FetchAllByRoleAsync(PlatformRole role)
    {
        return await _db.Accounts.Where(a => a.Role == role).ToListAsync();
    }

    public async Task<List<LearnerAccount>> FetchAllActiveAsync()
    {
        return await _db.Accounts.Where(a => a.IsActive).ToListAsync();
    }

    public async Task<List<LearnerAccount>> SearchByNameOrEmailAsync(string term)
    {
        return await _db.Accounts.Where(a => a.DisplayName.Contains(term) || a.EmailAddress.Contains(term)).ToListAsync();
    }

    public async Task RecordLoginTimestampAsync(int accountId)
    {
        //surgical update,don't load and re-save the full entity for a timestamp
        await _db.Accounts.Where(a => a.Id == accountId).ExecuteUpdateAsync(s => s.SetProperty(a => a.LastSeenAt,DateTime.UtcNow));
    }

    public async Task<LearnerAccount> PersistNewAccountAsync(LearnerAccount account)
    {
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task<LearnerAccount> SaveUpdatedAccountAsync(LearnerAccount account)
    {
        _db.Accounts.Update(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
