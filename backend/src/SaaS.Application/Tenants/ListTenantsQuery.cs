using MediatR;

namespace SaaS.Application.Tenants;

public record ListTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;
