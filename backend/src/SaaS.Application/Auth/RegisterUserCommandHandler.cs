using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaaS.Application.Keycloak;
using SaaS.Application.Provisioning;
using SaaS.Application.Tenants;
using SaaS.Domain.Entities;

namespace SaaS.Application.Auth;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserProvisioningService _provisioningService;
    private readonly IUserTermsAcceptanceRepository _termsRepository;
    private readonly Common.IEventPublisher _eventPublisher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IKeycloakAdminService keycloakAdmin,
        ITenantRepository tenantRepository,
        IUserProvisioningService provisioningService,
        IUserTermsAcceptanceRepository termsRepository,
        Common.IEventPublisher eventPublisher,
        IConfiguration configuration,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _keycloakAdmin = keycloakAdmin;
        _tenantRepository = tenantRepository;
        _provisioningService = provisioningService;
        _termsRepository = termsRepository;
        _eventPublisher = eventPublisher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration attempt for email {Email}", request.Email);

        if (!request.AcceptTerms)
            throw new InvalidOperationException("Acceptance of terms is required.");

        var existing = await _keycloakAdmin.FindUserByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("Registration failed: email already in use {Email}", request.Email);
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var defaultSlug = _configuration["Keycloak:DefaultTenantSlug"] ?? "default";
        var tenant = await _tenantRepository.GetBySlugAsync(defaultSlug, cancellationToken);
        if (tenant == null)
        {
            await _tenantRepository.EnsureDefaultTenantAsync(defaultSlug, "Default", cancellationToken);
            tenant = await _tenantRepository.GetBySlugAsync(defaultSlug, cancellationToken);
        }
        if (tenant == null)
            throw new InvalidOperationException("Default tenant is not available.");

        var defaultRole = _configuration["Keycloak:DefaultRole"];
        string keycloakUserId;
        try
        {
            keycloakUserId = await _keycloakAdmin.CreateUserAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                tenant.Id,
                defaultRole,
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Keycloak create user failed for {Email}", request.Email);
            throw;
        }

        await _provisioningService.ProvisionNewUserAsync(keycloakUserId, tenant.Id, defaultRole, cancellationToken);

        var termsAcceptance = UserTermsAcceptance.Create(keycloakUserId);
        await _termsRepository.AddAsync(termsAcceptance, cancellationToken);

        await _eventPublisher.PublishAsync(new UserRegisteredEvent(keycloakUserId, request.Email, tenant.Id), cancellationToken);
        _logger.LogInformation("User registered successfully: {Email}, KeycloakId {KeycloakUserId}", request.Email, keycloakUserId);

        return new RegisterUserResult();
    }
}

public record UserRegisteredEvent(string KeycloakUserId, string Email, Guid TenantId);
