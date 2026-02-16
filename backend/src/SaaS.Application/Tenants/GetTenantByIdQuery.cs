using MediatR;

namespace SaaS.Application.Tenants;

public record GetTenantByIdQuery(Guid TenantId) : IRequest<TenantDto?>;

public record TenantDto(Guid Id, string Name, string Slug, bool IsActive);
