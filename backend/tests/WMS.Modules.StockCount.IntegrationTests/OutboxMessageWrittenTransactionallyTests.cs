using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure;
using WMS.BuildingBlocks.Infrastructure.Outbox;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Domain;
using WMS.Modules.StockCount.Infrastructure;
using WMS.Modules.StockCount.Infrastructure.Persistence;

namespace WMS.Modules.StockCount.IntegrationTests;

/// <summary>
/// Proves OutboxWritingInterceptor lands the outbox row in the SAME transaction as the aggregate
/// write that raised the domain event — the entire point of the outbox pattern (bkz. CLAUDE.md
/// "Event-Driven"). No relay/BackgroundService involved here; that's covered separately by
/// WMS.Api.FunctionalTests, which exercises the real end-to-end pipeline.
/// </summary>
[Collection("Postgres")]
public sealed class OutboxMessageWrittenTransactionallyTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private ServiceProvider _serviceProvider = null!;

    public OutboxMessageWrittenTransactionallyTests(PostgresFixture fixture)
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
    public async Task ApprovingAnAdjustment_WritesAnUnprocessedOutboxRowInTheSameSaveChangesCall()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();

        using var scope = _serviceProvider.CreateScope();
        var adjustmentWriteRepository = scope.ServiceProvider.GetRequiredService<IStockCountAdjustmentWriteRepository>();

        var adjustment = StockCountAdjustment.Create(Guid.NewGuid(), Guid.NewGuid(), warehouseId, productId, -3m);
        adjustmentWriteRepository.Add(adjustment);
        await adjustmentWriteRepository.SaveChangesAsync(CancellationToken.None);

        adjustment.Approve(approvedByUserId);
        await adjustmentWriteRepository.SaveChangesAsync(CancellationToken.None);

        var dbContext = scope.ServiceProvider.GetRequiredService<StockCountDbContext>();
        var outboxMessages = await dbContext.Set<OutboxMessage>().ToListAsync();

        outboxMessages.Should().ContainSingle();
        var message = outboxMessages[0];
        message.ProcessedAtUtc.Should().BeNull();
        message.RetryCount.Should().Be(0);
        message.Type.Should().Contain(nameof(StockCountAdjustmentApprovedDomainEvent));

        var deserialized = (StockCountAdjustmentApprovedDomainEvent)JsonSerializer.Deserialize(
            message.Payload, Type.GetType(message.Type)!)!;
        deserialized.StockCountAdjustmentId.Should().Be(adjustment.Id);
        deserialized.WarehouseId.Should().Be(warehouseId);
        deserialized.ProductId.Should().Be(productId);
        deserialized.DifferenceQuantity.Should().Be(-3m);
    }
}
