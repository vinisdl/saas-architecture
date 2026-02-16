using System.Net;

namespace SaaS.API;

/// <summary>
/// Rewrites backchannel HTTP requests (metadata, JWKS) to use the configured Keycloak authority host.
/// When Keycloak is configured with KC_HOSTNAME=localhost, the discovery document returns URLs with
/// localhost, which the backend (running in Docker) cannot reach. This handler redirects those
/// requests to the authority host (e.g. keycloak:8080).
/// </summary>
internal sealed class KeycloakBackchannelHandler : DelegatingHandler
{
    private readonly Uri _authorityBase;

    public KeycloakBackchannelHandler(Uri authorityBase)
    {
        _authorityBase = authorityBase;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is { } uri && uri.PathAndQuery.StartsWith("/realms/", StringComparison.OrdinalIgnoreCase))
        {
            var rewritten = new Uri(_authorityBase, uri.PathAndQuery);
            if (rewritten.GetLeftPart(UriPartial.Authority) != uri.GetLeftPart(UriPartial.Authority))
            {
                var clone = CloneWithUri(request, rewritten);
                request.Dispose();
                request = clone;
            }
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static HttpRequestMessage CloneWithUri(HttpRequestMessage request, Uri newUri)
    {
        var clone = new HttpRequestMessage(request.Method, newUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy,
            Content = request.Content
        };
        foreach (var (key, value) in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(key, value);
        }
        foreach (var (key, value) in request.Options)
        {
            clone.Options.TryAdd(key, value);
        }
        return clone;
    }
}
