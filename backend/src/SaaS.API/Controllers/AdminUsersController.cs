using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Keycloak;
using SaaS.Application.Tenants;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly ITenantRepository _tenantRepository;

    public AdminUsersController(IKeycloakAdminService keycloakAdmin, ITenantRepository tenantRepository)
    {
        _keycloakAdmin = keycloakAdmin;
        _tenantRepository = tenantRepository;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<KeycloakUserDto>>> ListUsers(CancellationToken cancellationToken)
    {
        var users = await _keycloakAdmin.ListUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost("tenants/{tenantId:guid}/users/{keycloakUserId}")]
    public async Task<IActionResult> AssignUserToTenant(Guid tenantId, string keycloakUserId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
            return NotFound("Tenant not found.");

        await _keycloakAdmin.SetUserTenantAsync(keycloakUserId, tenantId, cancellationToken);
        return NoContent();
    }
}
