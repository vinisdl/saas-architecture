using MediatR;

namespace SaaS.Application.Tenants;

public record CreateTenantCommand(string Name, string Slug) : IRequest<CreateTenantResult>;

public record CreateTenantResult(Guid TenantId);
