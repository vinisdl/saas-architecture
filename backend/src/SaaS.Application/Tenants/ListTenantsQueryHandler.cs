using MediatR;

namespace SaaS.Application.Tenants;

public sealed class ListTenantsQueryHandler : IRequestHandler<ListTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public ListTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<IReadOnlyList<TenantDto>> Handle(ListTenantsQuery request, CancellationToken cancellationToken)
    {
        var list = await _tenantRepository.ListAsync(cancellationToken);
        return list.Select(t => new TenantDto(t.Id, t.Name, t.Slug, t.IsActive)).ToList();
    }
}
