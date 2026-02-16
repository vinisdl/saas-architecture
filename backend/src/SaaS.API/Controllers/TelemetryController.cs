using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaS.API.Controllers;

/// <summary>
/// Proxy OTLP para o frontend enviar traces ao Alloy/Grafana Cloud sem expor API key no browser.
/// </summary>
[ApiController]
[Route("api/telemetry")]
[AllowAnonymous]
public class TelemetryController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public TelemetryController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("v1/traces")]
    public async Task<IActionResult> ForwardTraces(CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["TelemetryProxy:AlloyOtlpHttpUrl"]?.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            return StatusCode(500, "TelemetryProxy:AlloyOtlpHttpUrl not configured");

        using var stream = new MemoryStream();
        await Request.Body.CopyToAsync(stream, cancellationToken);
        var body = stream.ToArray();
        if (body.Length == 0)
            return BadRequest();

        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/traces")
        {
            Content = new ByteArrayContent(body)
        };
        var contentType = Request.ContentType;
        if (!string.IsNullOrEmpty(contentType))
            request.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);

        var response = await client.SendAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode);
    }
}
