using Grafana.OpenTelemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaaS.Application;
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
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();
