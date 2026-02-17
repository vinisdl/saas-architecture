using SaaS.Domain.Entities;

namespace SaaS.Application.Provisioning;

public interface IUserTenantRepository
{
    Task<bool> ExistsAsync(string keycloakUserId, Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(UserTenant userTenant, CancellationToken cancellationToken = default);
}
