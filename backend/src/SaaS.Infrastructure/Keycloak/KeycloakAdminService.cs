using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaaS.Application.Keycloak;

namespace SaaS.Infrastructure.Keycloak;

public sealed class KeycloakAdminService : IKeycloakAdminService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakAdminService> _logger;
    private const string TenantIdAttribute = "tenant_id";

    public KeycloakAdminService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakAdminService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<KeycloakUserDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        var serverUrl = _configuration["Keycloak:Admin:ServerUrl"] ?? _configuration["Keycloak:Authority"]?.Replace("/realms/saas", "") ?? "http://localhost:8080";
        var realm = _configuration["Keycloak:Admin:Realm"] ?? "saas";

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{serverUrl.TrimEnd('/')}/admin/realms/{realm}/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var users = JsonSerializer.Deserialize<List<KeycloakUserRep>>(json);
        if (users == null)
            return Array.Empty<KeycloakUserDto>();

        return users.Select(u =>
        {
            Guid? tenantId = null;
            if (u.Attributes != null && u.Attributes.TryGetValue(TenantIdAttribute, out var values) && values?.Count > 0 && Guid.TryParse(values[0], out var tid))
                tenantId = tid;
            return new KeycloakUserDto(u.Id ?? "", u.Username, u.Email, tenantId);
        }).ToList();
    }

    public async Task SetUserTenantAsync(string keycloakUserId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        var serverUrl = _configuration["Keycloak:Admin:ServerUrl"] ?? _configuration["Keycloak:Authority"]?.Replace("/realms/saas", "") ?? "http://localhost:8080";
        var realm = _configuration["Keycloak:Admin:Realm"] ?? "saas";

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"{serverUrl.TrimEnd('/')}/admin/realms/{realm}/users/{keycloakUserId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getResponse = await _httpClient.SendAsync(getRequest, cancellationToken);
        getResponse.EnsureSuccessStatusCode();

        var userJson = await getResponse.Content.ReadAsStringAsync(cancellationToken);
        var user = JsonSerializer.Deserialize<KeycloakUserRep>(userJson);
        if (user == null)
            throw new InvalidOperationException("Failed to get user from Keycloak.");

        user.Attributes ??= new Dictionary<string, List<string>>();
        user.Attributes[TenantIdAttribute] = new List<string> { tenantId.ToString() };

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"{serverUrl.TrimEnd('/')}/admin/realms/{realm}/users/{keycloakUserId}");
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        putRequest.Content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

        var putResponse = await _httpClient.SendAsync(putRequest, cancellationToken);
        putResponse.EnsureSuccessStatusCode();
    }

    public async Task<KeycloakUserDto?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;
        var token = await GetAdminTokenAsync(cancellationToken);
        var serverUrl = GetServerUrl();
        var realm = GetRealm();
        var encodedEmail = Uri.EscapeDataString(email.Trim());
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{serverUrl}/admin/realms/{realm}/users?email={encodedEmail}&exact=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var users = JsonSerializer.Deserialize<List<KeycloakUserRep>>(json);
        var user = users?.FirstOrDefault();
        if (user == null)
            return null;
        Guid? tenantId = null;
        if (user.Attributes != null && user.Attributes.TryGetValue(TenantIdAttribute, out var values) && values?.Count > 0 && Guid.TryParse(values[0], out var tid))
            tenantId = tid;
        return new KeycloakUserDto(user.Id ?? "", user.Username, user.Email, tenantId);
    }

    public async Task<string> CreateUserAsync(string firstName, string lastName, string email, string password, Guid tenantId, string? defaultRole = null, CancellationToken cancellationToken = default)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        var serverUrl = GetServerUrl();
        var realm = GetRealm();
        var username = email.Trim();
        var createPayload = new Dictionary<string, object>
        {
            ["username"] = username,
            ["email"] = username,
            ["firstName"] = firstName?.Trim() ?? "",
            ["lastName"] = lastName?.Trim() ?? "",
            ["enabled"] = true,
            ["emailVerified"] = true,
            ["attributes"] = new Dictionary<string, List<string>> { [TenantIdAttribute] = new List<string> { tenantId.ToString() } },
            ["requiredActions"] = Array.Empty<string>(),
            ["credentials"] = new[]
            {
                new Dictionary<string, object> { ["type"] = "password", ["value"] = password, ["temporary"] = false }
            }
        };
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"{serverUrl}/admin/realms/{realm}/users");
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        createRequest.Content = new StringContent(JsonSerializer.Serialize(createPayload), Encoding.UTF8, "application/json");
        var createResponse = await _httpClient.SendAsync(createRequest, cancellationToken);
        if (createResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            throw new InvalidOperationException("User already exists with this username or email.");
        if (!createResponse.IsSuccessStatusCode)
        {
            var body = await createResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Keycloak create user failed: {StatusCode} {Body}", createResponse.StatusCode, body);
            createResponse.EnsureSuccessStatusCode();
        }
        var location = createResponse.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
            throw new InvalidOperationException("Keycloak did not return user location.");
        var userId = location.TrimEnd('/').Split('/').LastOrDefault();
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("Could not parse user id from Keycloak response.");
        if (!string.IsNullOrWhiteSpace(defaultRole))
        {
            var roleId = await GetRealmRoleIdAsync(token, serverUrl, realm, defaultRole, cancellationToken);
            if (roleId != null)
            {
                var rolePayload = new[] { new KeycloakRoleRep { Id = roleId, Name = defaultRole, ContainerId = realm } };
                using var roleRequest = new HttpRequestMessage(HttpMethod.Post, $"{serverUrl}/admin/realms/{realm}/users/{userId}/role-mappings/realm");
                roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                roleRequest.Content = new StringContent(JsonSerializer.Serialize(rolePayload), Encoding.UTF8, "application/json");
                var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
                if (!roleResponse.IsSuccessStatusCode)
                    _logger.LogWarning("Failed to assign default role {Role} to user {UserId}: {StatusCode}", defaultRole, userId, roleResponse.StatusCode);
                else
                    roleResponse.EnsureSuccessStatusCode();
            }
        }
        return userId;
    }

    public async Task EnsureUserProfileAndClearUpdateProfileAsync(string keycloakUserId, string? firstName, string? lastName, CancellationToken cancellationToken = default)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        var serverUrl = GetServerUrl();
        var realm = GetRealm();

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"{serverUrl}/admin/realms/{realm}/users/{keycloakUserId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getResponse = await _httpClient.SendAsync(getRequest, cancellationToken);
        if (!getResponse.IsSuccessStatusCode)
            return;
        var userJson = await getResponse.Content.ReadAsStringAsync(cancellationToken);
        var user = JsonSerializer.Deserialize<KeycloakUserRep>(userJson);
        if (user == null)
            return;

        var updated = false;
        if (!string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(user.FirstName))
        {
            user.FirstName = firstName.Trim();
            updated = true;
        }
        if (!string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(user.LastName))
        {
            user.LastName = lastName.Trim();
            updated = true;
        }
        var requiredActions = user.RequiredActions ?? new List<string>();
        var hasUpdateProfile = requiredActions.Any(a => string.Equals(a, "UPDATE_PROFILE", StringComparison.OrdinalIgnoreCase));
        if (hasUpdateProfile)
        {
            user.RequiredActions = requiredActions.Where(a => !string.Equals(a, "UPDATE_PROFILE", StringComparison.OrdinalIgnoreCase)).ToList();
            updated = true;
        }

        if (!updated)
            return;

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"{serverUrl}/admin/realms/{realm}/users/{keycloakUserId}");
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        putRequest.Content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
        var putResponse = await _httpClient.SendAsync(putRequest, cancellationToken);
        putResponse.EnsureSuccessStatusCode();
    }

    private async Task<string?> GetRealmRoleIdAsync(string token, string serverUrl, string realm, string roleName, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{serverUrl}/admin/realms/{realm}/roles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var roles = JsonSerializer.Deserialize<List<KeycloakRoleRep>>(json);
        var role = roles?.FirstOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));
        return role?.Id;
    }

    private string GetServerUrl() =>
        _configuration["Keycloak:Admin:ServerUrl"] ?? _configuration["Keycloak:Authority"]?.Replace("/realms/saas", "")?.TrimEnd('/') ?? "http://localhost:8080";
    private string GetRealm() => _configuration["Keycloak:Admin:Realm"] ?? "saas";

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var serverUrl = GetServerUrl();
        var realm = GetRealm();
        var clientId = _configuration["Keycloak:Admin:ClientId"] ?? "saas-backend";
        var clientSecret = _configuration["Keycloak:Admin:ClientSecret"] ?? "";

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };
        using var content = new FormUrlEncodedContent(form);
        var response = await _httpClient.PostAsync($"{serverUrl.TrimEnd('/')}/realms/{realm}/protocol/openid-connect/token", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(json);
        if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
            throw new InvalidOperationException("Failed to obtain Keycloak admin token.");
        return tokenResponse.AccessToken;
    }

    private sealed class KeycloakUserRep
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>>? Attributes { get; set; }
        [JsonPropertyName("requiredActions")]
        public List<string>? RequiredActions { get; set; }
    }

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class KeycloakRoleRep
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("containerId")]
        public string? ContainerId { get; set; }
    }
}
