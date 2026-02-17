namespace SaaS.Application.Keycloak;

public interface IKeycloakAdminService
{
    Task<IReadOnlyList<KeycloakUserDto>> ListUsersAsync(CancellationToken cancellationToken = default);
    Task SetUserTenantAsync(string keycloakUserId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<KeycloakUserDto?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<string> CreateUserAsync(string firstName, string lastName, string email, string password, Guid tenantId, string? defaultRole = null, CancellationToken cancellationToken = default);
    Task EnsureUserProfileAndClearUpdateProfileAsync(string keycloakUserId, string? firstName, string? lastName, CancellationToken cancellationToken = default);
}

public record KeycloakUserDto(string Id, string? Username, string? Email, Guid? TenantId);
