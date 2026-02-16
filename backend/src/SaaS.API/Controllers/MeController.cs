using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public MeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var adminUserId = _configuration["Keycloak:AdminUserId"] ?? "";
        var isAdmin = !string.IsNullOrEmpty(sub) && string.Equals(sub, adminUserId, StringComparison.OrdinalIgnoreCase);
        Console.WriteLine($"sub: {sub}, adminUserId: {adminUserId}, isAdmin: {isAdmin}");
        return Ok(new { sub, isAdmin });
    }
}
