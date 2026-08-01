using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inbound.Application.GoodsReceipts;
using WMS.Modules.Inbound.Domain;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.SharedKernel;

namespace WMS.Modules.Inbound.UnitTests;

public class GoodsReceiptApprovedDomainEventHandlerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly GoodsReceiptApprovedDomainEventHandler _handler;

    public GoodsReceiptApprovedDomainEventHandlerTests()
    {
        _sender.Send(Arg.Any<IncreaseStockCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _handler = new GoodsReceiptApprovedDomainEventHandler(_sender, Substitute.For<ILogger<GoodsReceiptApprovedDomainEventHandler>>());
    }

    [Fact]
    public async Task Handle_WithMultipleLines_SendsOneIncreaseStockCommandPerLineWithSequentialLineNumbers()
    {
        var warehouseId = Guid.NewGuid();
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        var domainEvent = new GoodsReceiptApprovedDomainEvent(
            Guid.NewGuid(),
            warehouseId,
            [new GoodsReceiptApprovedLine(productA, 5m), new GoodsReceiptApprovedLine(productB, 7m)],
            DateTimeOffset.UtcNow);
        var outboxMessageId = Guid.NewGuid();
        var notification = new DomainEventNotification<GoodsReceiptApprovedDomainEvent>(domainEvent, outboxMessageId);

        await _handler.Handle(notification, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<IncreaseStockCommand>(c =>
                c.SourceEventId == outboxMessageId && c.LineNumber == 0 && c.WarehouseId == warehouseId &&
                c.ProductId == productA && c.Quantity == 5m),
            Arg.Any<CancellationToken>());
        await _sender.Received(1).Send(
            Arg.Is<IncreaseStockCommand>(c =>
                c.SourceEventId == outboxMessageId && c.LineNumber == 1 && c.WarehouseId == warehouseId &&
                c.ProductId == productB && c.Quantity == 7m),
            Arg.Any<CancellationToken>());
    }
}
