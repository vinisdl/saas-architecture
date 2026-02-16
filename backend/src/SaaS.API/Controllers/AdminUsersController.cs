using System.Net;
using System.Net.Http;
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
        try
        {
            var users = await _keycloakAdmin.ListUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403"))
        {
            return StatusCode((int)HttpStatusCode.Forbidden,
                "Keycloak Admin API retornou 403. Atribua as roles view-users e manage-users (realm-management) ao service account do client saas-backend. Veja docs/keycloak-setup.md.");
        }
    }

    [HttpPost("tenants/{tenantId:guid}/users/{keycloakUserId}")]
    public async Task<IActionResult> AssignUserToTenant(Guid tenantId, string keycloakUserId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
            return NotFound("Tenant not found.");

        try
        {
            await _keycloakAdmin.SetUserTenantAsync(keycloakUserId, tenantId, cancellationToken);
            return NoContent();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403"))
        {
            return StatusCode((int)HttpStatusCode.Forbidden,
                "Keycloak Admin API retornou 403. Atribua a role manage-users (realm-management) ao service account do client saas-backend. Veja docs/keycloak-setup.md.");
        }
    }
}
