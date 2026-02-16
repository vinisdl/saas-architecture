namespace SaaS.Application.Keycloak;

public interface IKeycloakAdminService
{
    Task<IReadOnlyList<KeycloakUserDto>> ListUsersAsync(CancellationToken cancellationToken = default);
    Task SetUserTenantAsync(string keycloakUserId, Guid tenantId, CancellationToken cancellationToken = default);
}

public record KeycloakUserDto(string Id, string? Username, string? Email, Guid? TenantId);
