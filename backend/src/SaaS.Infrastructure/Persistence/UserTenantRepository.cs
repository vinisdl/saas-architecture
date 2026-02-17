using Microsoft.EntityFrameworkCore;
using SaaS.Application.Provisioning;
using SaaS.Domain.Entities;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Provisioning;

public sealed class UserTenantRepository : IUserTenantRepository
{
    private readonly AppDbContext _db;

    public UserTenantRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsAsync(string keycloakUserId, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.UserTenants.AnyAsync(
            x => x.KeycloakUserId == keycloakUserId && x.TenantId == tenantId,
            cancellationToken);

    public async Task AddAsync(UserTenant userTenant, CancellationToken cancellationToken = default)
    {
        await _db.UserTenants.AddAsync(userTenant, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
