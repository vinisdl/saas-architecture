namespace SaaS.Domain.Entities;

public class UserTermsAcceptance
{
    public Guid Id { get; private set; }
    public string KeycloakUserId { get; private set; } = string.Empty;
    public DateTime AcceptedAtUtc { get; private set; }
    public string TermsVersion { get; private set; } = "1.0";

    private UserTermsAcceptance() { }

    public static UserTermsAcceptance Create(string keycloakUserId, string termsVersion = "1.0")
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
            throw new ArgumentException("Keycloak user id is required.", nameof(keycloakUserId));
        return new UserTermsAcceptance
        {
            Id = Guid.NewGuid(),
            KeycloakUserId = keycloakUserId.Trim(),
            AcceptedAtUtc = DateTime.UtcNow,
            TermsVersion = termsVersion?.Trim() ?? "1.0"
        };
    }
}
