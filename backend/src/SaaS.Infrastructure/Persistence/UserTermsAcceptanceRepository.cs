using SaaS.Application.Provisioning;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Provisioning;

public sealed class UserTermsAcceptanceRepository : IUserTermsAcceptanceRepository
{
    private readonly AppDbContext _db;

    public UserTermsAcceptanceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(SaaS.Domain.Entities.UserTermsAcceptance acceptance, CancellationToken cancellationToken = default)
    {
        await _db.UserTermsAcceptances.AddAsync(acceptance, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
