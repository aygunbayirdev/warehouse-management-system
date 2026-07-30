using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Infrastructure;
using WMS.Modules.Inventory.Infrastructure.Persistence;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.IntegrationTests;

/// <summary>
/// Proves the xmin-based optimistic concurrency token (CLAUDE.md's "EF Core Kuralları") actually
/// rejects a stale write against a real Postgres instance — the single most safety-critical piece
/// of infrastructure in the system, since every workflow's stock update funnels through it.
/// </summary>
[Collection("Postgres")]
public sealed class StockItemConcurrencyTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private ServiceProvider _serviceProvider = null!;

    public StockItemConcurrencyTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _fixture.ConnectionString,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDomainEventDispatching();
        services.AddInventoryModule(configuration);
        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _serviceProvider.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_WhenTwoScopesModifyTheSameStockItemConcurrently_TheSecondSaveThrowsConcurrencyConflict()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        using (var setupScope = _serviceProvider.CreateScope())
        {
            var repository = setupScope.ServiceProvider.GetRequiredService<IStockItemWriteRepository>();
            var stockItem = await repository.GetOrCreateAsync(warehouseId, productId, CancellationToken.None);
            stockItem.Increase(10m);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        using var scopeA = _serviceProvider.CreateScope();
        using var scopeB = _serviceProvider.CreateScope();

        var repositoryA = scopeA.ServiceProvider.GetRequiredService<IStockItemWriteRepository>();
        var repositoryB = scopeB.ServiceProvider.GetRequiredService<IStockItemWriteRepository>();

        var stockItemA = await repositoryA.GetOrCreateAsync(warehouseId, productId, CancellationToken.None);
        var stockItemB = await repositoryB.GetOrCreateAsync(warehouseId, productId, CancellationToken.None);

        stockItemA.Increase(5m);
        await repositoryA.SaveChangesAsync(CancellationToken.None);

        stockItemB.Increase(3m);
        var act = async () => await repositoryB.SaveChangesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }
}
