using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Keycloak;
using SaaS.Application.Provisioning;
using SaaS.Application.Tenants;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly IUserProvisioningService _provisioningService;
    private readonly ITenantRepository _tenantRepository;

    public MeController(
        IConfiguration configuration,
        IKeycloakAdminService keycloakAdmin,
        IUserProvisioningService provisioningService,
        ITenantRepository tenantRepository)
    {
        _configuration = configuration;
        _keycloakAdmin = keycloakAdmin;
        _provisioningService = provisioningService;
        _tenantRepository = tenantRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(sub))
            return Ok(new { sub = (string?)null, isAdmin = false, tenantId = (Guid?)null });

        var givenName = User.FindFirstValue(ClaimTypes.GivenName) ?? User.FindFirstValue("given_name");
        var familyName = User.FindFirstValue(ClaimTypes.Surname) ?? User.FindFirstValue("family_name");
        try
        {
            await _keycloakAdmin.EnsureUserProfileAndClearUpdateProfileAsync(sub, givenName, familyName, cancellationToken);
        }
        catch
        {
            // Continue; profile update is best-effort
        }

        var tenantIdClaim = User.FindFirstValue("tenant_id");
        Guid? tenantId = null;
        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tid))
            tenantId = tid;

        if (!tenantId.HasValue)
        {
            var defaultSlug = _configuration["Keycloak:DefaultTenantSlug"] ?? "default";
            var tenant = await _tenantRepository.GetBySlugAsync(defaultSlug, cancellationToken);
            if (tenant != null)
            {
                try
                {
                    await _provisioningService.ProvisionNewUserAsync(sub, tenant.Id, null, cancellationToken);
                    tenantId = tenant.Id;
                }
                catch
                {
                    // Log and continue; token will not have tenant_id until next login
                }
            }
        }

        var adminUserId = _configuration["Keycloak:AdminUserId"] ?? "";
        var isAdmin = string.Equals(sub, adminUserId, StringComparison.OrdinalIgnoreCase);
        return Ok(new { sub, isAdmin, tenantId });
    }
}
