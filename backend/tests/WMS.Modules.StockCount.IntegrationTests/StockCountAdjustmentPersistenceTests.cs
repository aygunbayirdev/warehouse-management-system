using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Domain;
using WMS.Modules.StockCount.Infrastructure;
using WMS.Modules.StockCount.Infrastructure.Persistence;
using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;

namespace WMS.Modules.StockCount.IntegrationTests;

/// <summary>
/// StockCount and StockCountAdjustment are deliberately separate aggregate roots (bkz. sayim.md) —
/// this proves both persist correctly as independent rows in a real Postgres, linked by
/// StockCountId/StockCountLineId, rather than one being an owned/nested part of the other.
/// </summary>
[Collection("Postgres")]
public sealed class StockCountAdjustmentPersistenceTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private ServiceProvider _serviceProvider = null!;

    public StockCountAdjustmentPersistenceTests(PostgresFixture fixture)
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
        services.AddStockCountModule(configuration);
        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<StockCountDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _serviceProvider.DisposeAsync();

    [Fact]
    public async Task CompletingAStockCountWithAVariance_PersistsAnIndependentPendingAdjustmentRow()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        Guid stockCountId;

        using (var writeScope = _serviceProvider.CreateScope())
        {
            var stockCountWriteRepository = writeScope.ServiceProvider.GetRequiredService<IStockCountWriteRepository>();
            var adjustmentWriteRepository = writeScope.ServiceProvider.GetRequiredService<IStockCountAdjustmentWriteRepository>();

            var stockCount = StockCountAggregate.Create(warehouseId, Guid.NewGuid());
            stockCount.Start();
            stockCount.AddLine(productId, systemQuantity: 20m, countedQuantity: 17m);
            stockCountWriteRepository.Add(stockCount);
            await stockCountWriteRepository.SaveChangesAsync(CancellationToken.None);

            stockCountId = stockCount.Id;
            var line = stockCount.Lines.Single();

            stockCount.Complete();
            await stockCountWriteRepository.SaveChangesAsync(CancellationToken.None);

            var adjustment = StockCountAdjustment.Create(stockCount.Id, line.Id, warehouseId, productId, line.Difference);
            adjustmentWriteRepository.Add(adjustment);
            await adjustmentWriteRepository.SaveChangesAsync(CancellationToken.None);
        }

        using var readScope = _serviceProvider.CreateScope();
        var reloadedStockCount = await readScope.ServiceProvider
            .GetRequiredService<IStockCountWriteRepository>()
            .GetByIdAsync(stockCountId, CancellationToken.None);

        reloadedStockCount.Should().NotBeNull();
        reloadedStockCount!.Status.Should().Be(StockCountStatus.Completed);
        reloadedStockCount.Lines.Should().ContainSingle(line => line.Difference == -3m);

        var dbContext = readScope.ServiceProvider.GetRequiredService<StockCountDbContext>();
        var persistedAdjustments = await dbContext.Set<StockCountAdjustment>()
            .Where(adjustment => adjustment.StockCountId == stockCountId)
            .ToListAsync();

        persistedAdjustments.Should().ContainSingle();
        persistedAdjustments[0].Status.Should().Be(StockCountAdjustmentStatus.Pending);
        persistedAdjustments[0].DifferenceQuantity.Should().Be(-3m);
        persistedAdjustments[0].ProductId.Should().Be(productId);
    }
}
