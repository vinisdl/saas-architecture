using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SaaS.Application.Common;

namespace SaaS.Infrastructure.TenantContext;

public sealed class HttpTenantContext : ITenantContext
{
    private const string TenantIdClaim = "tenant_id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            if (_tenantId.HasValue)
                return _tenantId;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            var claim = httpContext.User.FindFirstValue(TenantIdClaim);
            if (!string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out var fromClaim))
            {
                _tenantId = fromClaim;
                return _tenantId;
            }

            return null;
        }
    }

    public bool HasTenant => TenantId.HasValue;
}
