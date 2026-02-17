using Grafana.OpenTelemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaaS.API;
using SaaS.Application;
using SaaS.Application.Tenants;
using SaaS.Infrastructure;
using SaaS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await Task.CompletedTask;
    };
    options.AddPolicy("register", context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddOpenTelemetry()
    .WithTracing(configure => configure.UseGrafana())
    .WithMetrics(configure => configure.UseGrafana());
builder.Logging.AddOpenTelemetry(options => options.UseGrafana());

var keycloakAuthority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/saas";
var keycloakAudience = builder.Configuration["Keycloak:Audience"] ?? "account";
var validIssuersSection = builder.Configuration.GetSection("Keycloak:ValidIssuers");
var validIssuersList = validIssuersSection.Get<string[]>();
var validIssuers = (validIssuersList != null && validIssuersList.Length > 0)
    ? validIssuersList.Select(u => u?.TrimEnd('/')).Where(u => !string.IsNullOrEmpty(u)).ToArray()
    : new[] { keycloakAuthority.TrimEnd('/') };

var authorityUri = new Uri(keycloakAuthority.TrimEnd('/'));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = keycloakAudience;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuers = validIssuers,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.BackchannelHttpHandler = new KeycloakBackchannelHandler(authorityUri);
    });

var adminUserId = builder.Configuration["Keycloak:AdminUserId"] ?? "";
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(ctx =>
        {
            var sub = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? ctx.User.FindFirst("sub")?.Value;
            return string.Equals(sub, adminUserId, StringComparison.OrdinalIgnoreCase);
        });
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var defaultTenantSlug = builder.Configuration["Keycloak:DefaultTenantSlug"] ?? "default";
    var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
    await tenantRepo.EnsureDefaultTenantAsync(defaultTenantSlug, "Default");
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();
