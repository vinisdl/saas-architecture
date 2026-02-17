using Microsoft.EntityFrameworkCore;
using SaaS.Application.Tenants;
using SaaS.Domain.Entities;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Tenants;

public sealed class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _db;

    public TenantRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _db.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _db.Tenants.AddAsync(tenant, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Tenants.AnyAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Tenants.OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task EnsureDefaultTenantAsync(string slug, string name, CancellationToken cancellationToken = default)
    {
        if (await _db.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))
            return;
        var tenant = Tenant.Create(name, slug);
        await _db.Tenants.AddAsync(tenant, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
