using MediatR;

namespace SaaS.Application.Tenants;

public record UpdateTenantCommand(Guid TenantId, string? Name, bool? IsActive) : IRequest<Unit>;
