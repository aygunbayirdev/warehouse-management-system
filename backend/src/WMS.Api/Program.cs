using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WMS.Api.Middleware;
using WMS.BuildingBlocks.Application;
using WMS.BuildingBlocks.Infrastructure;
using WMS.Modules.Catalog.Infrastructure;
using WMS.Modules.Catalog.Infrastructure.Persistence;
using WMS.Modules.Identity.Infrastructure;
using WMS.Modules.Identity.Infrastructure.Auth;
using WMS.Modules.Identity.Infrastructure.Seeding;
using WMS.Modules.Inbound.Infrastructure;
using WMS.Modules.Inbound.Infrastructure.Persistence;
using WMS.Modules.Inventory.Infrastructure;
using WMS.Modules.Inventory.Infrastructure.Persistence;
using WMS.Modules.Outbound.Infrastructure;
using WMS.Modules.Outbound.Infrastructure.Persistence;
using WMS.Modules.StockCount.Infrastructure;
using WMS.Modules.StockCount.Infrastructure.Persistence;
using WMS.Modules.Transfer.Infrastructure;
using WMS.Modules.Transfer.Infrastructure.Persistence;

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

    var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy => policy
            .WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
    });

    var app = builder.Build();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Every module owns its own DbContext/schema (bkz. CLAUDE.md §1), so each one needs its own
    // migration call. IdentitySeeder already migrates its own context as part of seeding the
    // default Admin user; the rest are applied here so a fresh database (e.g. `docker compose up`
    // against an empty Postgres volume) ends up with every schema in place without a separate
    // manual `dotnet ef database update` step per module.
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await services.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<InboundDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<OutboundDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<TransferDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<StockCountDbContext>().Database.MigrateAsync();
    }

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
