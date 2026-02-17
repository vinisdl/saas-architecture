namespace SaaS.Domain.Entities;

public class UserTenant
{
    public Guid Id { get; private set; }
    public string KeycloakUserId { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public string? Role { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private UserTenant() { }

    public static UserTenant Create(string keycloakUserId, Guid tenantId, string? role = null)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
            throw new ArgumentException("Keycloak user id is required.", nameof(keycloakUserId));
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));

        return new UserTenant
        {
            Id = Guid.NewGuid(),
            KeycloakUserId = keycloakUserId.Trim(),
            TenantId = tenantId,
            Role = role?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
