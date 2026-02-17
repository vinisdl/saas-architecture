using SaaS.Domain.Entities;

namespace SaaS.Application.Provisioning;

public interface IUserTermsAcceptanceRepository
{
    Task AddAsync(UserTermsAcceptance acceptance, CancellationToken cancellationToken = default);
}
