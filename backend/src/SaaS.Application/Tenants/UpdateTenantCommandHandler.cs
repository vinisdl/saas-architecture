using MediatR;

namespace SaaS.Application.Tenants;

public sealed class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Unit>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Unit> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new InvalidOperationException("Tenant not found.");

        if (request.Name != null)
            tenant.SetName(request.Name);
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                tenant.Activate();
            else
                tenant.Deactivate();
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        return Unit.Value;
    }
}
