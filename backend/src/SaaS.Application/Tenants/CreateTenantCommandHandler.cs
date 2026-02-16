using DomainEntities = SaaS.Domain.Entities;
using MediatR;

namespace SaaS.Application.Tenants;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, CreateTenantResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly Common.IEventPublisher _eventPublisher;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        Common.IEventPublisher eventPublisher)
    {
        _tenantRepository = tenantRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateTenantResult> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await _tenantRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException($"Tenant with slug '{request.Slug}' already exists.");

        var tenant = DomainEntities.Tenant.Create(request.Name, request.Slug);
        await _tenantRepository.AddAsync(tenant, cancellationToken);

        await _eventPublisher.PublishAsync(new TenantCreatedEvent(tenant.Id, tenant.Name, tenant.Slug), cancellationToken);

        return new CreateTenantResult(tenant.Id);
    }
}

public record TenantCreatedEvent(Guid TenantId, string Name, string Slug);
