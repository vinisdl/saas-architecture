using SaaS.Application.Keycloak;
using SaaS.Application.Provisioning;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Provisioning;

public sealed class UserProvisioningService : IUserProvisioningService
{
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly IUserTenantRepository _userTenantRepository;

    public UserProvisioningService(IKeycloakAdminService keycloakAdmin, IUserTenantRepository userTenantRepository)
    {
        _keycloakAdmin = keycloakAdmin;
        _userTenantRepository = userTenantRepository;
    }

    public async Task ProvisionNewUserAsync(string keycloakUserId, Guid tenantId, string? role = null, CancellationToken cancellationToken = default)
    {
        await _keycloakAdmin.SetUserTenantAsync(keycloakUserId, tenantId, cancellationToken);
        var exists = await _userTenantRepository.ExistsAsync(keycloakUserId, tenantId, cancellationToken);
        if (!exists)
        {
            var userTenant = UserTenant.Create(keycloakUserId, tenantId, role);
            await _userTenantRepository.AddAsync(userTenant, cancellationToken);
        }
    }
}
