namespace SaaS.Application.Provisioning;

public interface IUserProvisioningService
{
    Task ProvisionNewUserAsync(string keycloakUserId, Guid tenantId, string? role = null, CancellationToken cancellationToken = default);
}
