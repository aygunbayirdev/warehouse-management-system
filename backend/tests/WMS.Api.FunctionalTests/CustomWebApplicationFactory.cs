using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace WMS.Api.FunctionalTests;

/// <summary>
/// Boots the real WMS.Api host (all seven modules, real MediatR pipeline, real JWT auth) against an
/// ephemeral Postgres container instead of the developer's local database. Program.cs's own startup
/// migration step (added for `docker compose up` against a fresh volume, bkz. TASKS.md Faz 13) runs
/// unmodified here, so this doubles as a check that a brand new database boots correctly.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Program.cs reads ConnectionStrings:Default into a local variable *before* builder.Build()
        // runs, so a ConfigureAppConfiguration override (applied at Build()-time by
        // WebApplicationFactory's host interception) arrives too late to affect it. Environment
        // variables are read as part of WebApplication.CreateBuilder()'s own default configuration
        // setup instead, which happens earlier - before any of Program.cs's own code - so this is
        // the one override mechanism that actually lands in time.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _container.GetConnectionString());

        // DemoDataSeeder runs on every fresh database by default (see TASKS.md Faz 15), which would
        // otherwise collide with GoodsReceiptWorkflowTests' own "ADET" unit of measure. Functional
        // tests want a clean, minimal database they fully control, not the demo dataset.
        Environment.SetEnvironmentVariable("Seeding__SeedDemoData", "false");
    }

    async Task IAsyncLifetime.DisposeAsync() => await _container.DisposeAsync();
}
