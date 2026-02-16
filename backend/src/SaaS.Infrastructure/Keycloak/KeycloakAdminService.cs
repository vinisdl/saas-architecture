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

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var serverUrl = _configuration["Keycloak:Admin:ServerUrl"] ?? _configuration["Keycloak:Authority"]?.Replace("/realms/saas", "") ?? "http://localhost:8080";
        var realm = _configuration["Keycloak:Admin:Realm"] ?? "saas";
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
        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>>? Attributes { get; set; }
    }

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
