using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Inventory.Infrastructure;
using WMS.Modules.Inventory.Infrastructure.Persistence;

namespace WMS.Modules.Inventory.IntegrationTests;

/// <summary>
/// Proves the (SourceEventId, LineNumber) idempotency guard closes the "outbox delivers at-least-once"
/// gap for real: sending the SAME IncreaseStockCommand twice (simulating the outbox relay redelivering
/// a message it already applied before crashing) must only mutate stock once.
/// </summary>
[Collection("Postgres")]
public sealed class ProcessedDomainEventIdempotencyTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private ServiceProvider _serviceProvider = null!;

    public ProcessedDomainEventIdempotencyTests(PostgresFixture fixture)
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
        services.AddDomainEventOutbox();
        services.AddInventoryModule(configuration);
        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _serviceProvider.DisposeAsync();

    [Fact]
    public async Task SendingTheSameIncreaseStockCommandTwice_OnlyAppliesItOnce()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sourceEventId = Guid.NewGuid();
        var command = new IncreaseStockCommand(sourceEventId, 0, warehouseId, productId, 10m, "test redelivery");

        using var firstScope = _serviceProvider.CreateScope();
        var firstResult = await firstScope.ServiceProvider.GetRequiredService<ISender>().Send(command, CancellationToken.None);

        using var secondScope = _serviceProvider.CreateScope();
        var secondResult = await secondScope.ServiceProvider.GetRequiredService<ISender>().Send(command, CancellationToken.None);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();

        using var readScope = _serviceProvider.CreateScope();
        var dbContext = readScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var stockItem = dbContext.StockItems.Single(item => item.WarehouseId == warehouseId && item.ProductId == productId);
        stockItem.Quantity.Should().Be(10m);

        var ledgerRows = dbContext.ProcessedDomainEvents.Where(p => p.SourceEventId == sourceEventId).ToList();
        ledgerRows.Should().ContainSingle();
    }
}
