using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WMS.Api.Middleware;
using WMS.BuildingBlocks.Application;
using WMS.BuildingBlocks.Infrastructure;
using WMS.Modules.Catalog.Infrastructure;
using WMS.Modules.Identity.Infrastructure;
using WMS.Modules.Identity.Infrastructure.Auth;
using WMS.Modules.Identity.Infrastructure.Seeding;
using WMS.Modules.Inbound.Infrastructure;
using WMS.Modules.Inventory.Infrastructure;
using WMS.Modules.Outbound.Infrastructure;
using WMS.Modules.StockCount.Infrastructure;
using WMS.Modules.Transfer.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Cross-module building blocks: MediatR pipeline behaviors, EF Core domain-event dispatch,
    // and the shared Dapper connection factory used by every module's read side.
    builder.Services.AddApplicationBehaviors();
    builder.Services.AddDomainEventDispatching();

    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

    builder.Services.AddSqlConnectionFactory(connectionString);

    // Module composition roots. Each AddXxxModule() wires up that module's own DbContext,
    // repositories, and MediatR/FluentValidation registrations (no-op until the module has code).
    builder.Services
        .AddIdentityModule(builder.Configuration)
        .AddCatalogModule(builder.Configuration)
        .AddInventoryModule(builder.Configuration)
        .AddInboundModule(builder.Configuration)
        .AddOutboundModule(builder.Configuration)
        .AddTransferModule(builder.Configuration)
        .AddStockCountModule(builder.Configuration);

    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
        ?? throw new InvalidOperationException("Jwt configuration section is missing.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = "sub",
                RoleClaimType = JwtTokenService.RoleClaimType,
            };
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await IdentitySeeder.SeedAsync(app.Services);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "WMS.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
